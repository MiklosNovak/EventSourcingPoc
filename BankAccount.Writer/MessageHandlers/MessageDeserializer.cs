using System.Collections.Concurrent;
using System.Text;
using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rebus.Extensions;
using Rebus.Messages;
using Rebus.Serialization;

namespace BankAccount.Writer.MessageHandlers;

public class MessageDeserializer : ISerializer
{
    public static readonly IReadOnlyDictionary<string, Type> MessageTypes = new ConcurrentDictionary<string, Type>
    {
        [nameof(AccountCreatedCommand)] = typeof(AccountCreatedCommand),
        [nameof(MoneyDepositedCommand)] = typeof(MoneyDepositedCommand),
        [nameof(MoneyWithdrawnCommand)] = typeof(MoneyWithdrawnCommand)
    };

    private readonly ISerializer _serializer;

    public MessageDeserializer(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public Task<TransportMessage> Serialize(Message message) => _serializer.Serialize(message);

    public async Task<Message> Deserialize(TransportMessage transportMessage)
    {
        return await Task.Run(() => DeserializeSync(transportMessage)).ConfigureAwait(false);
    }

    private static Message DeserializeSync(TransportMessage transportMessage)
    {
        var headers = transportMessage.Headers.Clone();

        var json = Encoding.UTF8.GetString(transportMessage.Body);

        var typeName = headers.GetValue(Headers.Type);

        if (!MessageTypes.TryGetValue(typeName, out var type))
        {
            return new Message(headers, JsonConvert.DeserializeObject<JObject>(json));
        }

        var body = JsonConvert.DeserializeObject(json, type);

        return new Message(headers, body);
    }
}
