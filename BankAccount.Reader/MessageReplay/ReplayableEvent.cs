namespace BankAccount.Reader.MessageReplay;

// this is just a technical class to be able to add the ExpiryDate property to the event, this isnt part of the integration event
public record ReplayableEvent
{
    public DateTime? ExpiryDate { get; set; }
}