using System.Text;
using BankAccount.Writer.MessageHandlers;
using BankAccount.Writer.MessageHandlers.MoneyDeposited;
using BankAccount.Writer.MessageHandlers.UserCreated;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Rebus.Messages;
using Rebus.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BankAccount.Writer.Tests.MessageHandlers;

[TestClass]
public class MessageDeserializerTests
{
    private readonly ISerializer _serializer = Substitute.For<ISerializer>();
    private readonly MessageDeserializer _messageDeserializer;

    public MessageDeserializerTests()
    {
        _messageDeserializer = new MessageDeserializer(_serializer);
    }

    [TestMethod]
    public async Task Serialize_ShouldReturnTransportMessage()
    {
        // Arrange
        var message = new Message([], new UserCreatedEvent());
        var transportMessage = new TransportMessage([], []);
        _messageDeserializer.Serialize(message).Returns(transportMessage);

        // Act
        var result = await _messageDeserializer.Serialize(message);

        // Assert
        result.Should().Be(transportMessage);
    }

    [TestMethod]
    public async Task Deserialize_ShouldReturnRawJson_WhenMessageTypeIsNotRegistered()
    {
        // Arrange
        var rawJson = "{\"key\":\"value\"}";
        var headers = new Dictionary<string, string> { { Headers.Type, "UnknownEvent" } };
        var transportMessage = new TransportMessage(headers, Encoding.UTF8.GetBytes(rawJson));

        _serializer.Deserialize(Arg.Any<TransportMessage>()).Returns(Task.FromResult(new Message(headers, JObject.Parse(rawJson))));

        // Act
        var result = await _messageDeserializer.Deserialize(transportMessage);

        // Assert
        result.Body.Should().BeEquivalentTo(JObject.Parse(rawJson));
        result.Headers[Headers.Type].Should().Be("UnknownEvent");
    }

    [TestMethod]
    public async Task Deserialize_ShouldDeserializeCorrectMessage_WhenValidEventIsPassed()
    {
        // Arrange
        var message = new MoneyDepositedEvent { AccountId = "user@example.com", Amount = 100m };
        var headers = new Dictionary<string, string> { { Headers.Type, nameof(MoneyDepositedEvent) } };
        var jsonBody = JsonConvert.SerializeObject(message);
        var transportMessage = new TransportMessage(headers, Encoding.UTF8.GetBytes(jsonBody));

        // Act
        var result = await _messageDeserializer.Deserialize(transportMessage);

        // Assert
        var deserializedMessage = (MoneyDepositedEvent) result.Body;
        deserializedMessage.Should().NotBeNull();
        deserializedMessage.AccountId.Should().Be(message.AccountId);
        deserializedMessage.Amount.Should().Be(message.Amount);
    }
}
