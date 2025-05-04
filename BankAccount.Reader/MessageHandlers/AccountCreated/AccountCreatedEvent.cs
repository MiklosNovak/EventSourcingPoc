using BankAccount.Reader.MessageReplay;

namespace BankAccount.Reader.MessageHandlers.AccountCreated;

public record AccountCreatedEvent : ReplayableEvent
{
    public string AccountId { get; init; }

    public int Version { get; init; }
}
