using ClearPay.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Tests;

public sealed class RabbitOutboxPublisherTests
{
    [Fact]
    public async Task Null_connection_falls_back_without_throwing()
    {
        var fallback = new LoggingOutboxPublisher(NullLogger<LoggingOutboxPublisher>.Instance);
        var sut = new RabbitOutboxPublisher(null, NullLogger<RabbitOutboxPublisher>.Instance, fallback);

        var act = async () => await sut.PublishAsync("transfer.completed", "{}", Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
