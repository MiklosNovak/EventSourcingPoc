using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.DomainEvents;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BankAccount.Writer.Tests.AccountLogic;

[TestClass]
public class AccountTests
{
    [TestMethod]
    public void Constructor_ShouldRaiseAccountCreatedEvent_WhenEmailIsValid()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var account = new Account(email);

        // Assert
        account.AccountId.Should().Be(email);
        account.Balance.Should().Be(0);
        account.GetUncommittedEvents.Should().ContainSingle(e => e.DomainEvent is AccountCreatedEvent);
    }

    [TestMethod]
    public void Deposit_ShouldIncreaseBalanceAndRaiseEvent_WhenAmountIsValid()
    {
        // Arrange
        var account = new Account("user@example.com");

        // Act
        account.Deposit(100);

        // Assert
        account.Balance.Should().Be(100);
        account.GetUncommittedEvents.Should().ContainSingle(e => e.DomainEvent is AccountCreditedEvent);
    }

    [TestMethod]
    public void Withdrawn_ShouldDecreaseBalanceAndRaiseEvent_WhenAmountIsValid()
    {
        // Arrange
        var account = new Account("user@example.com");
        account.Deposit(200);

        // Act
        account.Withdrawn(100);

        // Assert
        account.Balance.Should().Be(100);
        account.GetUncommittedEvents.Should().Contain(e => e.DomainEvent is AccountDebitedEvent);
    }

    [TestMethod]
    public void Withdrawn_ShouldThrow_WhenAmountIsGreaterThanBalance()
    {
        // Arrange
        var account = new Account("user@example.com");
        account.Deposit(50);

        // Act
        var act = () => account.Withdrawn(100);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*insufficient funds*");
    }

    [TestMethod]
    public void Deposit_ShouldThrow_WhenAmountIsZero()
    {
        // Arrange
        var account = new Account("user@example.com");

        // Act
        var act = () => account.Deposit(0);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*greater than 0*");
    }

    [TestMethod]
    public void Deposit_ShouldThrow_WhenAmountWouldExceedLimit()
    {
        // Arrange
        var account = new Account("user@example.com");
        account.Deposit(9000);

        // Act
        var act = () => account.Deposit(2000);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*maximum balance*");
    }

    [TestMethod]
    public void ClearUncommittedEvents_ShouldClearAllUncommittedEvents()
    {
        // Arrange
        var account = new Account("user@example.com");
        account.Deposit(100);

        // Act
        account.ClearUncommittedEvents();

        // Assert
        account.GetUncommittedEvents.Should().BeEmpty();
    }

    [TestMethod]
    public void Rehydrate_ShouldRestoreStateFromEvents()
    {
        // Arrange
        var events = new IAccountDomainEvent[]
        {
            new AccountCreatedEvent { AccountId = "user@example.com" },
            new AccountCreditedEvent { AccountId = "user@example.com", Amount = 100 },
            new AccountDebitedEvent { AccountId = "user@example.com", Amount = 30 }
        };

        // Act
        var account = Account.Rehydrate(events);

        // Assert
        account.AccountId.Should().Be("user@example.com");
        account.Balance.Should().Be(70);
        account.GetUncommittedEvents.Should().BeEmpty();
    }
}

