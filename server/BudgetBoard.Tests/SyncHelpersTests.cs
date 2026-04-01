using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class SyncHelpersTests
{
    [Fact]
    public async Task SyncBalance_WhenBalanceIsNewer_ShouldCreateNewBalance()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 1000.00m,
            DateTime = DateTime.UtcNow,
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(helper.demoUser.Id, newBalanceRequest),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncBalance_WhenBalanceExistsForSameDate_ShouldUpdateExistingBalance()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var existingDate = DateTime.UtcNow.Date;
        var existingBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = existingDate,
        };
        helper.UserDataContext.Balances.Add(existingBalance);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 1000.00m,
            DateTime = existingDate.AddHours(12), // Same date, different time
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ =>
                _.UpdateBalanceAsync(
                    helper.demoUser.Id,
                    It.Is<IBalanceUpdateRequest>(req =>
                        req.ID == existingBalance.ID && req.Amount == 1000.00m
                    )
                ),
            Times.Once
        );
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncBalance_WhenBalanceIsOlder_ShouldNotCreateOrUpdate()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var newerDate = DateTime.UtcNow;
        var existingBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 1000.00m,
            DateTime = newerDate,
        };
        helper.UserDataContext.Balances.Add(existingBalance);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var olderBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = newerDate.AddDays(-5), // Older than existing
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            olderBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
        balanceServiceMock.Verify(
            _ => _.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<IBalanceUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncBalance_WhenAccountNotFound_ShouldReturnError()
    {
        // Arrange
        var helper = new TestHelper();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newBalanceRequest = new BalanceCreateRequest
        {
            AccountID = Guid.NewGuid(), // Non-existent account
            Amount = 1000.00m,
            DateTime = DateTime.UtcNow,
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().NotBeNull();
        error!.Value.ErrorKey.Should().Be("AccountNotFoundError");
        error.Value.ErrorParams.Should().Contain(newBalanceRequest.AccountID.ToString());
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncBalance_WhenAccountHasNoBalances_ShouldCreateNewBalance()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = DateTime.UtcNow.AddDays(-30), // Older date, but no existing balances
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(helper.demoUser.Id, newBalanceRequest),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncBalance_WhenBalanceIsNewerThanExisting_ShouldCreateNewBalance()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var olderDate = DateTime.UtcNow.AddDays(-10);
        var existingBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = olderDate,
        };
        helper.UserDataContext.Balances.Add(existingBalance);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newerBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 1000.00m,
            DateTime = DateTime.UtcNow, // Newer than existing
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newerBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(helper.demoUser.Id, newerBalanceRequest),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncBalance_WhenBalanceExistsForSameDateDifferentTime_ShouldUpdateWithNewAmount()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var existingDate = new DateTime(2024, 1, 15, 10, 0, 0);
        var existingBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = existingDate,
        };
        helper.UserDataContext.Balances.Add(existingBalance);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var newBalanceRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 750.00m,
            DateTime = new DateTime(2024, 1, 15, 18, 30, 0), // Same date, different time
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            newBalanceRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        existingBalance.Amount.Should().Be(750.00m);
        balanceServiceMock.Verify(
            _ =>
                _.UpdateBalanceAsync(
                    helper.demoUser.Id,
                    It.Is<IBalanceUpdateRequest>(req =>
                        req.ID == existingBalance.ID
                        && req.Amount == 750.00m
                        && req.AccountID == account.ID
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncBalance_WhenMultipleBalancesExist_ShouldCompareWithLatest()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var oldBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 100.00m,
            DateTime = DateTime.UtcNow.AddDays(-20),
        };

        var latestBalance = new Balance
        {
            ID = Guid.NewGuid(),
            AccountID = account.ID,
            Amount = 500.00m,
            DateTime = DateTime.UtcNow.AddDays(-5),
        };

        helper.UserDataContext.Balances.AddRange(oldBalance, latestBalance);
        await helper.UserDataContext.SaveChangesAsync();

        var balanceServiceMock = new Mock<IBalanceService>();

        var olderThanLatestRequest = new BalanceCreateRequest
        {
            AccountID = account.ID,
            Amount = 300.00m,
            DateTime = DateTime.UtcNow.AddDays(-10), // Older than latest but newer than old
        };

        // Act
        var error = await SyncHelpers.SyncBalance(
            helper.demoUser,
            olderThanLatestRequest,
            balanceServiceMock.Object
        );

        // Assert
        error.Should().BeNull();
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
        balanceServiceMock.Verify(
            _ => _.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<IBalanceUpdateRequest>()),
            Times.Never
        );
    }
}
