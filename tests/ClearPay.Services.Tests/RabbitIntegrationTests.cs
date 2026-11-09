using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using ClearPay.Contracts;
using ClearPay.Messaging;
using ClearPay.Notifications;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;

namespace ClearPay.Services.Tests;

/// <summary>Runs only when <c>CLEARPAY_TEST_RABBITMQ</c> holds an AMQP URI (CI service container or a local throwaway broker).</summary>
public sealed class RabbitFactAttribute : FactAttribute
{
    public const string Variable = "CLEARPAY_TEST_RABBITMQ";

    public RabbitFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
            Skip = $"Set {Variable} (e.g. amqp://guest:guest@localhost:5672/) to run against a real RabbitMQ.";
    }
}

/// <summary>
/// The real broker behaviour the unit tests cannot prove: confirms, topic routing, TTL retry queue with
/// x-death counting, DLQ, consumer idempotency on a duplicate publish, and trace context across the hop.
/// Every test uses its own queue and routing key and deletes them afterwards.
/// </summary>
public sealed class RabbitIntegrationTests : IAsyncLifetime
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    private readonly string _queue = "test." + Guid.NewGuid().ToString("N")[..10];
    private readonly Script _handler = new();
    private RabbitConnectionProvider _connections = null!;
    private RabbitEventPublisher _publisher = null!;
    private ServiceProvider _services = null!;
    private RabbitConsumerService<ScriptedHandler>? _consumer;
    private SqliteDb? _sqlite;

    private string RoutingKey => _queue + ".happened";

    public Task InitializeAsync()
    {
        var uri = Environment.GetEnvironmentVariable(RabbitFactAttribute.Variable);
        _connections = new RabbitConnectionProvider(uri, "clearpay-tests", NullLogger<RabbitConnectionProvider>.Instance);
        _publisher = new RabbitEventPublisher(_connections, NullLogger<RabbitEventPublisher>.Instance);
        _sqlite = new SqliteDb();
        var sqlite = _sqlite;
        _services = new ServiceCollection()
            .AddSingleton(_handler)
            .AddScoped<ScriptedHandler>()
            .AddScoped(_ => sqlite.Notifications())
            .BuildServiceProvider();
        return Task.CompletedTask;
    }

    [RabbitFact]
    public async Task Failing_handler_is_retried_through_the_ttl_queue_then_dead_lettered()
    {
        _handler.Behaviour = _ => throw new TimeoutException("downstream timeout");
        await StartConsumerAsync(maxAttempts: 3);

        var id = Guid.NewGuid();
        await _publisher.PublishAsync(Message(id));

        var dead = await WaitForDeadLetterAsync();
        _handler.Attempts.ToList().Select(a => a.Attempt).Should().Equal(1, 2, 3);
        _handler.Attempts.ToList().Should().OnlyContain(a => a.MessageId == id);
        dead.BasicProperties.MessageId.Should().Be(id.ToString());
        dead.BasicProperties.Type.Should().Be(RoutingKey);
        Encoding.UTF8.GetString((byte[])dead.BasicProperties.Headers[MessagingNames.ErrorHeader])
            .Should().Contain("TimeoutException").And.Contain("downstream timeout");
        Convert.ToInt32(dead.BasicProperties.Headers[MessagingNames.AttemptsHeader]).Should().Be(3);
    }

    [RabbitFact]
    public async Task Poison_message_skips_the_retries()
    {
        _handler.Behaviour = _ => throw new PoisonMessageException("cannot read body");
        await StartConsumerAsync(maxAttempts: 5);

        await _publisher.PublishAsync(Message(Guid.NewGuid()));

        await WaitForDeadLetterAsync();
        _handler.Attempts.ToList().Should().ContainSingle();
    }

    [RabbitFact]
    public async Task Same_message_id_published_twice_is_processed_once()
    {
        _handler.Behaviour = async (scope) =>
        {
            var db = scope.Services.GetRequiredService<NotificationsDbContext>();
            await IdempotentProcessing.RunOnceAsync(db, _queue, scope.Message, _ =>
            {
                db.NotificationLogs.Add(new NotificationLog
                {
                    Id = Guid.NewGuid(),
                    MessageId = scope.Message.MessageId,
                    EventType = scope.Message.Type,
                    Channel = "Webhook",
                    Recipient = "test",
                    Subject = "test",
                    CreatedAt = DateTimeOffset.UtcNow
                });
                return Task.CompletedTask;
            });
        };
        await StartConsumerAsync(maxAttempts: 3);

        var message = Message(Guid.NewGuid());
        await _publisher.PublishAsync(message);
        await _publisher.PublishAsync(message); // outbox re-send after a crash between publish and "mark Sent"

        await WaitUntilAsync(() => _handler.Attempts.Count >= 2);
        await using var db = _sqlite!.Notifications();
        (await db.NotificationLogs.CountAsync()).Should().Be(1);
        (await db.ProcessedMessages.CountAsync()).Should().Be(1);
    }

    [RabbitFact]
    public async Task Consumer_span_continues_the_trace_of_the_outbox_row()
    {
        var spans = new ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == MessagingTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add
        };
        ActivitySource.AddActivityListener(listener);
        _handler.Behaviour = _ => Task.CompletedTask;
        await StartConsumerAsync(maxAttempts: 3);

        // The traceparent the web request stored on the outbox row.
        var requestTrace = ActivityTraceId.CreateRandom();
        var traceParent = $"00-{requestTrace}-{ActivitySpanId.CreateRandom()}-01";
        await _publisher.PublishAsync(Message(Guid.NewGuid()) with { TraceParent = traceParent });

        await WaitUntilAsync(() => spans.Any(s => s.Kind == ActivityKind.Consumer && s.TraceId == requestTrace));
        var publish = spans.Single(s => s.Kind == ActivityKind.Producer && s.TraceId == requestTrace);
        var consume = spans.Single(s => s.Kind == ActivityKind.Consumer && s.TraceId == requestTrace);
        consume.ParentSpanId.Should().Be(publish.SpanId);
    }

    private OutgoingMessage Message(Guid id) =>
        new(id, RoutingKey, "{\"hello\":\"world\"}", id, DateTimeOffset.UtcNow);

    private async Task StartConsumerAsync(int maxAttempts)
    {
        _consumer = new RabbitConsumerService<ScriptedHandler>(
            _connections,
            _services.GetRequiredService<IServiceScopeFactory>(),
            new ConsumerQueueOptions
            {
                QueueName = _queue,
                BindingKeys = [RoutingKey],
                MaxAttempts = maxAttempts,
                RetryDelay = TimeSpan.FromMilliseconds(300)
            },
            NullLogger<RabbitConsumerService<ScriptedHandler>>.Instance);
        await _consumer.StartAsync(CancellationToken.None);
        // Wait until the queue exists and is bound, so the first publish is routed.
        await WaitUntilAsync(() =>
        {
            using var channel = _connections.GetConnection().CreateModel();
            try
            {
                return channel.QueueDeclarePassive(_queue).ConsumerCount > 0;
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
            {
                return false;
            }
        });
    }

    private async Task<BasicGetResult> WaitForDeadLetterAsync()
    {
        BasicGetResult? result = null;
        await WaitUntilAsync(() =>
        {
            using var channel = _connections.GetConnection().CreateModel();
            result = channel.BasicGet(_queue + MessagingNames.DeadLetterSuffix, autoAck: true);
            return result is not null;
        });
        return result!;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.Elapsed > Timeout)
                throw new TimeoutException("Condition not met within " + Timeout);
            await Task.Delay(100);
        }
    }

    public async Task DisposeAsync()
    {
        if (_consumer is not null)
        {
            await _consumer.StopAsync(CancellationToken.None);
            _consumer.Dispose();
        }

        if (_connections.IsConfigured)
        {
            try
            {
                using var channel = _connections.GetConnection().CreateModel();
                channel.QueueDelete(_queue);
                channel.QueueDelete(_queue + MessagingNames.RetrySuffix);
                channel.QueueDelete(_queue + MessagingNames.DeadLetterSuffix);
            }
            catch (BrokerUnavailableException)
            {
                // Broker gone: nothing to clean up.
            }
        }

        _publisher.Dispose();
        _connections.Dispose();
        await _services.DisposeAsync();
        _sqlite?.Dispose();
    }

    /// <summary>What the handler does in this test, plus every attempt it saw. Singleton.</summary>
    public sealed class Script
    {
        public ConcurrentQueue<IncomingMessage> Attempts { get; } = new();

        public Func<HandlerCall, Task> Behaviour { get; set; } = _ => Task.CompletedTask;
    }

    /// <summary>Resolved per message in the consumer's DI scope, like the real handlers.</summary>
    public sealed class ScriptedHandler : IMessageHandler
    {
        private readonly Script _script;
        private readonly IServiceProvider _scope;

        public ScriptedHandler(Script script, IServiceProvider scope)
        {
            _script = script;
            _scope = scope;
        }

        public Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken)
        {
            _script.Attempts.Enqueue(message);
            return _script.Behaviour(new HandlerCall(message, _scope));
        }
    }

    public sealed record HandlerCall(IncomingMessage Message, IServiceProvider Services);
}
