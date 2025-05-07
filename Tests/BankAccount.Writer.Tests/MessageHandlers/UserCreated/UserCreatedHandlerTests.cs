using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.MessageHandlers.UserCreated;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankAccount.Writer.Tests.MessageHandlers.UserCreated;

[TestClass]
public class UserCreatedHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IOutboxEventRepository _outboxEventRepository = Substitute.For<IOutboxEventRepository>();
    private readonly IAccountUnitOfWork _unitOfWork = Substitute.For<IAccountUnitOfWork>();
    private readonly UserCreatedHandler _handler;

    public UserCreatedHandlerTests()
    {
        _unitOfWork.AccountRepository.Returns(_accountRepository);
        _unitOfWork.OutboxEventRepository.Returns(_outboxEventRepository);
        _handler = new UserCreatedHandler(_unitOfWork);
    }

    [TestMethod]
    public async Task Handle_ShouldCreateAccountAndCommit_WhenAccountDoesNotExist()
    {
        // Arrange
        var message = new UserCreatedEvent { AccountId = "newuser@example.com" };
        _accountRepository.GetAsync(message.AccountId).Returns((Account)null);

        // Act
        await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await _outboxEventRepository.Received(1).AddAsync(
            Arg.Is<IReadOnlyCollection<VersionedDomainEvent>>(events => events.Any())).ConfigureAwait(false);

        await _accountRepository.Received(1).SaveAsync(Arg.Is<Account>(a => a.AccountId == message.AccountId)).ConfigureAwait(false);
        await _unitOfWork.Received(1).CommitAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldThrowAndRollback_WhenAccountAlreadyExists()
    {
        // Arrange
        var message = new UserCreatedEvent { AccountId = "existing@example.com" };
        _accountRepository.GetAsync(message.AccountId).Returns(new Account(message.AccountId));

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account with email 'existing@example.com' already exists.").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Handle_ShouldRollback_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var message = new UserCreatedEvent { AccountId = "user@example.com" };
        _accountRepository.GetAsync(message.AccountId).ThrowsAsync(new Exception("Unexpected"));

        // Act
        var act = async () => await _handler.Handle(message).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected").ConfigureAwait(false);
        await _unitOfWork.Received(1).RollbackAsync().ConfigureAwait(false);
    }
}
