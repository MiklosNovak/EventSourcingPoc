using System.Collections.Concurrent;
using System.Text;
using BankAccount.Reader.MessageHandlers.AccountCreated;
using BankAccount.Reader.MessageHandlers.AccountCredited;
using BankAccount.Reader.MessageHandlers.AccountDebited;
using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rebus.Extensions;
using Rebus.Messages;
using Rebus.Serialization;

namespace BankAccount.Reader.MessageHandlers;

public class MessageDeserializer : ISerializer
{
    public static readonly IReadOnlyDictionary<string, Type> MessageTypes = new ConcurrentDictionary<string, Type>
    {
        [nameof(AccountCreatedEvent)] = typeof(AccountCreatedEvent),
        [nameof(AccountCreditedEvent)] = typeof(AccountCreditedEvent),
        [nameof(AccountDebitedEvent)] = typeof(AccountDebitedEvent),
        [nameof(AccountStateCorruptedEvent)] = typeof(AccountStateCorruptedEvent),
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
