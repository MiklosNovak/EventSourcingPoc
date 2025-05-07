using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.MessageHandlers.AccountReplyRequested;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessageHandlers.AccountReplyRequested;

[TestClass]
public class AccountReplyRequestedEventHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IAccountUnitOfWork _unitOfWork = Substitute.For<IAccountUnitOfWork>();
    private readonly AccountReplyRequestedEventHandler _handler;

    public AccountReplyRequestedEventHandlerTests()
    {
        _unitOfWork.AccountRepository.Returns(_accountRepository);
        _unitOfWork.OutboxEventRepository.Returns(_outboxEventRepository);

        _handler = new AccountReplyRequestedEventHandler(_unitOfWork);
    }

    [TestMethod]
    public async Task Handle_ShouldAddEventsAndCommit_WhenAccountExists()
    {
        // Arrange
        var message = new AccountReplyRequestedEvent { AccountId = "user@example.com" };
        var account = new Account("user@example.com");

        _accountRepository.GetAsync(message.AccountId).Returns(account);

        // Act
        await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await _outboxEventRepository.Received(1).AddAsync(Arg.Is<IReadOnlyCollection<VersionedDomainEvent>>(s => s.SequenceEqual(account.GetVersionedDomainEvents))).ConfigureAwait(false);
        await _unitOfWork.Received(1).CommitAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldDoNothing_WhenAccountDoesNotExist()
    {
        // Arrange
        var message = new AccountReplyRequestedEvent { AccountId = "unknown@example.com" };
        _accountRepository.GetAsync(message.AccountId).Returns((Account)null);

        // Act
        await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await _outboxEventRepository.DidNotReceive().AddAsync(Arg.Any<IReadOnlyCollection<VersionedDomainEvent>>()).ConfigureAwait(false);
        await _unitOfWork.Received(1).CommitAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldRollback_WhenExceptionOccurs()
    {
        // Arrange
        var message = new AccountReplyRequestedEvent { AccountId = "user@example.com" };
        _accountRepository.GetAsync(message.AccountId).ThrowsAsync(_ => throw new Exception("DB error"));

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("DB error").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }
}
