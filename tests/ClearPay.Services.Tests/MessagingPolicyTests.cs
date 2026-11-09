using System.Text;
using System.Text.Json;
using ClearPay.Messaging;
using FluentAssertions;

namespace ClearPay.Services.Tests;

public sealed class MessagingPolicyTests
{
    [Fact]
    public void Death_header_counts_only_rejections_from_the_work_queue()
    {
        // Shape the RabbitMQ client really delivers: List<object> of tables, strings as byte[], count as long.
        var headers = new Dictionary<string, object>
        {
            ["x-death"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["queue"] = Encoding.UTF8.GetBytes("notifications.events.retry"),
                    ["reason"] = Encoding.UTF8.GetBytes("expired"),
                    ["count"] = 2L
                },
                new Dictionary<string, object>
                {
                    ["queue"] = Encoding.UTF8.GetBytes("notifications.events"),
                    ["reason"] = Encoding.UTF8.GetBytes("rejected"),
                    ["count"] = 2L
                }
            }
        };

        DeathHeader.RejectedCount(headers, "notifications.events").Should().Be(2);
        DeathHeader.RejectedCount(headers, "other.queue").Should().Be(0);
        DeathHeader.RejectedCount(null, "notifications.events").Should().Be(0);
        DeathHeader.RejectedCount(new Dictionary<string, object>(), "notifications.events").Should().Be(0);
    }

    [Theory]
    [InlineData(1, 3, FailureAction.Retry)]
    [InlineData(2, 3, FailureAction.Retry)]
    [InlineData(3, 3, FailureAction.DeadLetter)]
    [InlineData(1, 1, FailureAction.DeadLetter)]
    public void Transient_errors_are_retried_until_max_attempts(int attempt, int max, FailureAction expected) =>
        ConsumerFailurePolicy.Decide(new TimeoutException("sql timeout"), attempt, max).Should().Be(expected);

    [Fact]
    public void Poison_goes_to_the_dlq_on_the_first_attempt()
    {
        ConsumerFailurePolicy.Decide(new PoisonMessageException("bad"), 1, 3).Should().Be(FailureAction.DeadLetter);
        ConsumerFailurePolicy.Decide(new JsonException("bad json"), 1, 3).Should().Be(FailureAction.DeadLetter);
    }

    [Fact]
    public void Consumer_queue_names_follow_the_convention()
    {
        var options = new ConsumerQueueOptions { QueueName = "notifications.events" };
        options.RetryQueue.Should().Be("notifications.events.retry");
        options.DeadLetterQueue.Should().Be("notifications.events.dlq");
    }
}
