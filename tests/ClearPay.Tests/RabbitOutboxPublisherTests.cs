using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Messaging;
using ClearPay.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Tests;

public sealed class RabbitOutboxPublisherTests
{
    private static OutboxEnvelope Envelope() =>
        new(Guid.NewGuid(), "clearpay.transfer.completed", "{}", Guid.NewGuid(), DateTimeOffset.UtcNow);

    [Fact]
    public async Task No_broker_configured_uses_the_log_publisher_without_throwing()
    {
        var sut = new LoggingOutboxPublisher(NullLogger<LoggingOutboxPublisher>.Instance);

        var act = async () => await sut.PublishAsync(Envelope());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Configured_but_unreachable_broker_throws_so_the_outbox_row_stays_pending()
    {
        // Port 1 on loopback: nothing listens, the connect fails fast.
        using var provider = new RabbitConnectionProvider(
            "amqp://guest:guest@127.0.0.1:1/", "test", NullLogger<RabbitConnectionProvider>.Instance);
        using var inner = new RabbitEventPublisher(provider, NullLogger<RabbitEventPublisher>.Instance);
        var sut = new RabbitOutboxPublisher(inner);

        var act = async () => await sut.PublishAsync(Envelope());

        await act.Should().ThrowAsync<BrokerUnavailableException>();
        provider.Status.Should().Be("down");
    }

    [Fact]
    public void Health_status_is_off_without_a_connection_string()
    {
        using var provider = new RabbitConnectionProvider("", "test", NullLogger<RabbitConnectionProvider>.Instance);
        new RabbitRuntimeStatus(provider).Value.Should().Be("off");
    }
}
