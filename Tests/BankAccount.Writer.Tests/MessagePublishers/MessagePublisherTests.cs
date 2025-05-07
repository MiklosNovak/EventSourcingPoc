using BankAccount.Writer.MessagePublishers;
using BankAccount.Writer.Repositories.OutboxEvents;
using Microsoft.Extensions.Logging;
using Rebus.Messages;
using NSubstitute;
using Newtonsoft.Json.Linq;
using Rebus.Bus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessagePublishers;

[TestClass]
public class MessagePublisherTests
{
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly ILogger<MessagePublisher> _logger = Substitute.For<ILogger<MessagePublisher>>();
    private readonly MessagePublisher _messagePublisher;

    public MessagePublisherTests()
    {
        _messagePublisher = new MessagePublisher(_outboxEventRepository, _bus, _logger);
    }

    [TestMethod]
    public async Task PublishMessagesAsync_ShouldPublishMessages_WhenUnprocessedEventsExist()
    {
        // Arrange
        var unProcessedEvent = new OutboxEventEntity
        {
            EventType = "MoneyDeposited",
            Data = "{\"Amount\": 100}",
            Version = 33
        };

        var events = new List<OutboxEventEntity> { unProcessedEvent };
        _outboxEventRepository.GetUnProcessedAsync(Arg.Any<int>()).Returns(events);

        // Act
        await _messagePublisher.PublishMessagesAsync().ConfigureAwait(false);

        // Assert
        await _bus.Advanced.Topics.Received(1).Publish(
            "MoneyDeposited", 
            Arg.Is<JObject>(j => j["Amount"]!.Value<int>() == 100 && j["Version"]!.Value<int>() == 33), 
            Arg.Is<Dictionary<string, string>>(s=> s[Headers.Type] == unProcessedEvent.EventType)).ConfigureAwait(false);
        await _outboxEventRepository.Received(1).MarkAsProcessedAsync(unProcessedEvent).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PublishMessagesAsync_ShouldNotPublishMessages_WhenNoUnProcessedEvents()
    {
        // Arrange
        _outboxEventRepository.GetUnProcessedAsync(Arg.Any<int>()).Returns([]);

        // Act
        await _messagePublisher.PublishMessagesAsync().ConfigureAwait(false);

        // Assert
        await _bus.Advanced.Topics.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<JObject>(), Arg.Any<Dictionary<string, string>>()).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PublishMessagesAsync_ShouldLogError_WhenPublishFails()
    {
        // Arrange
        var unProcessedEvent = new OutboxEventEntity
        {
            EventType = "MoneyDeposited",
            Data = "{\"Amount\": 100}",
            Version = 1
        };

        var events = new List<OutboxEventEntity> { unProcessedEvent };
        _outboxEventRepository.GetUnProcessedAsync(Arg.Any<int>()).Returns(events);

        // Simulate an error while publishing
        _bus.Advanced.Topics.Publish(Arg.Any<string>(), Arg.Any<JObject>(), Arg.Any < Dictionary<string, string>>())
            .ThrowsAsync(new Exception("Publish error"));

        // Act
        await _messagePublisher.PublishMessagesAsync().ConfigureAwait(false);

        // Assert
        _logger.Received(1).LogError(Arg.Any<Exception>(), "Failed to publish outbox event: MoneyDeposited, version: 1");
    }

    [TestMethod]
    public async Task PublishMessagesAsync_ShouldHandleErrorWhenMarkAsProcessedFails()
    {
        // Arrange
        var unProcessedEvent = new OutboxEventEntity
        {
            EventType = "MoneyDeposited",
            Data = "{\"Amount\": 100}",
            Version = 1
        };

        var events = new List<OutboxEventEntity> { unProcessedEvent };
        _outboxEventRepository.GetUnProcessedAsync(Arg.Any<int>()).Returns(events);

        // Simulate an error in MarkAsProcessedAsync
        _outboxEventRepository.MarkAsProcessedAsync(Arg.Any<OutboxEventEntity>())
            .Throws(new Exception("Error marking as processed"));

        // Act
        await _messagePublisher.PublishMessagesAsync().ConfigureAwait(false);

        // Assert
        _logger.Received(1).LogError(Arg.Any<Exception>(), "Failed to publish outbox event: MoneyDeposited, version: 1");
    }
}