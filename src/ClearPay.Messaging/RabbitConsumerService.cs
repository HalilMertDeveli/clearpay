using System.Text;
using System.Text.Json;
using ClearPay.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClearPay.Messaging;

/// <summary>What the consumer does with a message whose handler threw.</summary>
public enum FailureAction
{
    /// <summary>Nack without requeue: the work queue dead-letters it into the TTL retry queue.</summary>
    Retry,

    /// <summary>Copy to the <c>.dlq</c> queue (with the error in a header), then ack the original.</summary>
    DeadLetter
}

public static class ConsumerFailurePolicy
{
    /// <summary>
    /// Poison (bad JSON, no message id) never gets better, so it skips the retries.
    /// Anything else (SQL timeout, a downstream outage, a bug) gets <paramref name="maxAttempts"/> tries in total.
    /// </summary>
    public static FailureAction Decide(Exception exception, int attempt, int maxAttempts) =>
        exception is PoisonMessageException or JsonException || attempt >= maxAttempts
            ? FailureAction.DeadLetter
            : FailureAction.Retry;
}

/// <summary>
/// Hosted consumer for one queue. Per message: start a consumer span from the <c>traceparent</c> header,
/// resolve <typeparamref name="THandler"/> in a new DI scope, then ack / retry / dead-letter.
/// Manual acks only, so a crash mid-handler means redelivery, never loss.
/// Reconnects with a fixed back-off while the broker is down.
/// </summary>
public sealed class RabbitConsumerService<THandler> : BackgroundService
    where THandler : class, IMessageHandler
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

    private readonly RabbitConnectionProvider _connections;
    private readonly IServiceScopeFactory _scopes;
    private readonly ConsumerQueueOptions _options;
    private readonly ILogger<RabbitConsumerService<THandler>> _logger;

    public RabbitConsumerService(
        RabbitConnectionProvider connections,
        IServiceScopeFactory scopes,
        ConsumerQueueOptions options,
        ILogger<RabbitConsumerService<THandler>> logger)
    {
        _connections = connections;
        _scopes = scopes;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_connections.IsConfigured)
        {
            _logger.LogWarning("RabbitMQ is not configured; consumer for {Queue} is idle", _options.QueueName);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeUntilChannelClosesAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Consumer {Queue} lost the broker ({Reason}); reconnecting in {Delay}s",
                    _options.QueueName, ex.InnerException?.Message ?? ex.Message, ReconnectDelay.TotalSeconds);
            }

            try
            {
                await Task.Delay(ReconnectDelay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ConsumeUntilChannelClosesAsync(CancellationToken stoppingToken)
    {
        using var channel = _connections.GetConnection().CreateModel();
        RabbitTopology.DeclareConsumerQueue(channel, _options);
        channel.BasicQos(prefetchSize: 0, prefetchCount: _options.Prefetch, global: false);
        channel.ConfirmSelect(); // dead-letter copies are confirmed before the original is acked

        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        channel.ModelShutdown += (_, args) => closed.TrySetResult();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += (_, delivery) => OnReceivedAsync(channel, delivery, stoppingToken);
        var consumerTag = channel.BasicConsume(_options.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation(
            "Consuming {Queue} (bindings {Bindings}, max attempts {MaxAttempts}, retry delay {RetryDelay}s)",
            _options.QueueName,
            string.Join(", ", _options.BindingKeys),
            _options.MaxAttempts,
            _options.RetryDelay.TotalSeconds);

        var stopped = Task.Delay(Timeout.Infinite, stoppingToken);
        await Task.WhenAny(closed.Task, stopped).ConfigureAwait(false);
        if (stoppingToken.IsCancellationRequested)
        {
            if (channel.IsOpen)
                channel.BasicCancel(consumerTag);
            stoppingToken.ThrowIfCancellationRequested();
        }

        throw new BrokerUnavailableException("Channel closed: " + channel.CloseReason?.ReplyText);
    }

    private async Task OnReceivedAsync(IModel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        var props = delivery.BasicProperties;
        var type = string.IsNullOrEmpty(props.Type) ? delivery.RoutingKey : props.Type;
        var attempt = (int)Math.Min(int.MaxValue - 1, DeathHeader.RejectedCount(props.Headers, _options.QueueName)) + 1;

        using var activity = MessageTraceContext.StartConsume(_options.QueueName, type, props.Headers);
        activity?.SetTag("messaging.message.id", props.MessageId);
        activity?.SetTag("clearpay.attempt", attempt);
        var traceId = activity?.TraceId.ToString() ?? MessageTraceContext.Extract(props.Headers).TraceId.ToString();
        // Template scope: structured sinks get MessageId / MessageType as properties, the console prints it readably.
        using var logScope = _logger.BeginScope("message {MessageId} {MessageType}", props.MessageId, type);

        try
        {
            if (!Guid.TryParse(props.MessageId, out var messageId))
                throw new PoisonMessageException("MessageId property is missing or not a Guid.");

            var message = new IncomingMessage(
                messageId,
                type,
                Encoding.UTF8.GetString(delivery.Body.Span),
                Guid.TryParse(props.CorrelationId, out var correlation) ? correlation : null,
                attempt);

            await using (var scope = _scopes.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await handler.HandleAsync(message, stoppingToken).ConfigureAwait(false);
            }

            channel.BasicAck(delivery.DeliveryTag, multiple: false);
            _logger.LogInformation(
                "Handled {MessageType} {MessageId} on {Queue} (attempt {Attempt}, trace {TraceId})",
                type, messageId, _options.QueueName, attempt, traceId);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down: no ack, the broker redelivers it to the next consumer.
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.GetType().Name);
            if (ConsumerFailurePolicy.Decide(ex, attempt, _options.MaxAttempts) == FailureAction.Retry)
            {
                _logger.LogWarning(
                    "Attempt {Attempt}/{MaxAttempts} of {MessageType} {MessageId} failed ({Error}); retrying in {Delay}s via {RetryQueue}",
                    attempt, _options.MaxAttempts, type, props.MessageId, ex.Message,
                    _options.RetryDelay.TotalSeconds, _options.RetryQueue);
                channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
            }
            else
            {
                DeadLetter(channel, delivery, ex, attempt);
                _logger.LogError(
                    "{MessageType} {MessageId} moved to {DeadLetterQueue} after attempt {Attempt}: {ErrorType} {Error}",
                    type, props.MessageId, _options.DeadLetterQueue, attempt, ex.GetType().Name, ex.Message);
            }
        }
    }

    private void DeadLetter(IModel channel, BasicDeliverEventArgs delivery, Exception error, int attempt)
    {
        var original = delivery.BasicProperties;
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = original.MessageId;
        props.Type = original.Type;
        props.CorrelationId = original.CorrelationId;
        props.ContentType = original.ContentType;
        props.Timestamp = original.Timestamp;
        props.Headers = original.Headers is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(original.Headers);
        var reason = $"{error.GetType().Name}: {error.Message}";
        props.Headers[MessagingNames.ErrorHeader] = reason.Length > 500 ? reason[..500] : reason;
        props.Headers[MessagingNames.AttemptsHeader] = attempt;

        channel.BasicPublish(string.Empty, _options.DeadLetterQueue, mandatory: false, basicProperties: props, body: delivery.Body);
        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
        channel.BasicAck(delivery.DeliveryTag, multiple: false);
    }
}

public static class MessagingServiceCollectionExtensions
{
    /// <summary>Connection provider + confirming publisher. <paramref name="amqpUri"/> empty = broker off.</summary>
    public static IServiceCollection AddClearPayRabbit(this IServiceCollection services, string? amqpUri, string clientName)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(sp => new RabbitConnectionProvider(
            amqpUri, clientName, sp.GetRequiredService<ILogger<RabbitConnectionProvider>>()));
        services.AddSingleton<RabbitEventPublisher>(sp => new RabbitEventPublisher(
            sp.GetRequiredService<RabbitConnectionProvider>(),
            sp.GetRequiredService<ILogger<RabbitEventPublisher>>()));
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RabbitEventPublisher>());
        return services;
    }

    /// <summary>Registers <typeparamref name="THandler"/> (scoped) and a hosted consumer for its queue.</summary>
    public static IServiceCollection AddRabbitConsumer<THandler>(this IServiceCollection services, ConsumerQueueOptions options)
        where THandler : class, IMessageHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.AddScoped<THandler>();
        services.AddHostedService(sp => new RabbitConsumerService<THandler>(
            sp.GetRequiredService<RabbitConnectionProvider>(),
            sp.GetRequiredService<IServiceScopeFactory>(),
            options,
            sp.GetRequiredService<ILogger<RabbitConsumerService<THandler>>>()));
        return services;
    }
}
