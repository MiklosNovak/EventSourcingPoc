﻿namespace BankAccount.Writer.DomainEvents;

public interface IAccountDomainEvent
{
    public string AccountId { get; }    
}
