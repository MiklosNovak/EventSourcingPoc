using BankAccount.Reader.MessageReplay;

namespace BankAccount.Reader.MessageHandlers.AccountCredited;

public record AccountCreditedEvent : ReplayableEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }

    public int Version { get; init; }
}