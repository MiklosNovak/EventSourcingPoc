using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.MessageHandlers.MoneyDeposited;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessageHandlers.MoneyDeposited;

[TestClass]
public class MoneyDepositedHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IAccountUnitOfWork _unitOfWork = Substitute.For<IAccountUnitOfWork>();
    private readonly MoneyDepositedHandler _handler;

    public MoneyDepositedHandlerTests()
    {
        _unitOfWork.AccountRepository.Returns(_accountRepository);
        _unitOfWork.OutboxEventRepository.Returns(_outboxEventRepository);

        _handler = new MoneyDepositedHandler(_unitOfWork);
    }

    [TestMethod]
    public async Task Handle_ShouldDepositMoneyAndCommit_WhenAccountExists()
    {
        // Arrange
        var message = new MoneyDepositedEvent
        {
            AccountId = "user@example.com",
            Amount = 100
        };

        var account = new Account("user@example.com");

        _accountRepository.GetAsync(message.AccountId).Returns(account);

        // Act
        await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await _outboxEventRepository.Received(1).AddAsync(Arg.Is<IReadOnlyCollection<VersionedDomainEvent>>(e => e.SequenceEqual(account.GetUncommittedEvents))).ConfigureAwait(false);
        await _accountRepository.Received(1).SaveAsync(account).ConfigureAwait(false);
        await _unitOfWork.Received(1).CommitAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldThrowAndRollback_WhenAccountNotFound()
    {
        // Arrange
        var message = new MoneyDepositedEvent
        {
            AccountId = "nonexistent@example.com",
            Amount = 50
        };

        _accountRepository.GetAsync(message.AccountId).Returns((Account)null);

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account 'nonexistent@example.com' not found!").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldRollback_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var message = new MoneyDepositedEvent
        {
            AccountId = "user@example.com",
            Amount = 100
        };

        _accountRepository.GetAsync(message.AccountId).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected error").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }
}
