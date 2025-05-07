using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.MessageHandlers.MoneyTransferred;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessageHandlers.MoneyTransferred;

[TestClass]
public class MoneyTransferredHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IAccountUnitOfWork _unitOfWork = Substitute.For<IAccountUnitOfWork>();
    private readonly MoneyTransferredHandler _handler;

    public MoneyTransferredHandlerTests()
    {
        _unitOfWork.AccountRepository.Returns(_accountRepository);
        _unitOfWork.OutboxEventRepository.Returns(_outboxEventRepository);

        _handler = new MoneyTransferredHandler(_unitOfWork);
    }

    [TestMethod]
    public async Task Handle_ShouldTransferMoneyAndCommit_WhenBothAccountsExist()
    {
        // Arrange
        var message = new MoneyTransferredEvent
        {
            AccountId = "user1@example.com",
            TargetAccountId = "user2@example.com",
            Amount = 100
        };

        var sourceAccount = new Account(message.AccountId);
        sourceAccount.Deposit(200);

        var targetAccount = new Account(message.TargetAccountId);

        _accountRepository.GetAsync(message.AccountId).Returns(sourceAccount);
        _accountRepository.GetAsync(message.TargetAccountId).Returns(targetAccount);

        // Act
        await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await _outboxEventRepository.Received(1).AddAsync(
            Arg.Is<IReadOnlyCollection<VersionedDomainEvent>>(events =>
                events.SequenceEqual(sourceAccount.GetUncommittedEvents.Concat(targetAccount.GetUncommittedEvents))
            )).ConfigureAwait(false);

        await _accountRepository.Received(1).SaveAsync(sourceAccount).ConfigureAwait(false);
        await _accountRepository.Received(1).SaveAsync(targetAccount).ConfigureAwait(false);
        await _unitOfWork.Received(1).CommitAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldThrowAndRollback_WhenSourceAccountNotFound()
    {
        // Arrange
        var message = new MoneyTransferredEvent
        {
            AccountId = "notfound@example.com",
            TargetAccountId = "user2@example.com",
            Amount = 100
        };

        _accountRepository.GetAsync(message.AccountId).Returns((Account)null);

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account 'notfound@example.com' not found!").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldThrowAndRollback_WhenTargetAccountNotFound()
    {
        // Arrange
        var message = new MoneyTransferredEvent
        {
            AccountId = "user1@example.com",
            TargetAccountId = "notfound@example.com",
            Amount = 100
        };

        var sourceAccount = new Account(message.AccountId);
        sourceAccount.Deposit(200);

        _accountRepository.GetAsync(message.AccountId).Returns(sourceAccount);
        _accountRepository.GetAsync(message.TargetAccountId).Returns((Account)null);

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Target account 'notfound@example.com' not found!").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldRollback_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var message = new MoneyTransferredEvent
        {
            AccountId = "user1@example.com",
            TargetAccountId = "user2@example.com",
            Amount = 50
        };

        _accountRepository.GetAsync(message.AccountId).ThrowsAsync(new Exception("Unexpected failure"));

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected failure").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }
}
