using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessageHandlers.MoneyWithdrawn;

[TestClass]
public class MoneyWithdrawnHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IAccountUnitOfWork _unitOfWork = Substitute.For<IAccountUnitOfWork>();
    private readonly MoneyWithdrawnHandler _handler;

    public MoneyWithdrawnHandlerTests()
    {
        _unitOfWork.AccountRepository.Returns(_accountRepository);
        _unitOfWork.OutboxEventRepository.Returns(_outboxEventRepository);

        _handler = new MoneyWithdrawnHandler(_unitOfWork);
    }

    [TestMethod]
    public async Task Handle_ShouldWithdrawMoneyAndCommit_WhenAccountExists()
    {
        // Arrange
        var message = new MoneyWithdrawnEvent
        {
            AccountId = "user@example.com",
            Amount = 100
        };

        var account = new Account(message.AccountId);
        account.Deposit(200); // Ensure sufficient balance

        _accountRepository.GetAsync(message.AccountId).Returns(account);

        // Act
        await _handler.Handle(message);

        // Assert
        await _outboxEventRepository.Received(1).AddAsync(
            Arg.Is<IReadOnlyCollection<VersionedDomainEvent>>(events =>
                events.SequenceEqual(account.GetUncommittedEvents))
        );

        await _accountRepository.Received(1).SaveAsync(account);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [TestMethod]
    public async Task Handle_ShouldThrowAndRollback_WhenAccountDoesNotExist()
    {
        // Arrange
        var message = new MoneyWithdrawnEvent
        {
            AccountId = "unknown@example.com",
            Amount = 50
        };

        _accountRepository.GetAsync(message.AccountId).Returns((Account)null);

        // Act
        var act = async () => await _handler.Handle(message);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account 'unknown@example.com' not found!");
        await _unitOfWork.Received(1).RollbackAsync();
    }

    [TestMethod]
    public async Task Handle_ShouldRollback_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var message = new MoneyWithdrawnEvent
        {
            AccountId = "user@example.com",
            Amount = 10
        };

        _accountRepository.GetAsync(message.AccountId).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var act = async () => await _handler.Handle(message);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected error");
        await _unitOfWork.Received(1).RollbackAsync();
    }
}
