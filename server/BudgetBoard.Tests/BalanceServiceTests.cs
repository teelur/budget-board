using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class BalanceServiceTests
{
    #region CreateBalanceAsync
    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithValidData_ShouldCreateBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceCreateRequest = new BalanceCreateRequest
        {
            Amount = 1000,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AccountID = account.ID,
        };

        // Act
        await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        helper.UserDataContext.Balances.Should().ContainSingle();
        helper.UserDataContext.Balances.Single().Should().BeEquivalentTo(balanceCreateRequest);
    }

    [Fact]
    public async Task CreateBalancesAsync_WhenBalanceExistsForSameDate_ShouldUpdateExistingBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var existingBalance = new BalanceFaker([account.ID]).Generate();
        account.Balances.Add(existingBalance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceCreateRequest = new BalanceCreateRequest
        {
            Amount = existingBalance.Amount + 100,
            Date = existingBalance.Date,
            AccountID = account.ID,
        };

        // Act
        await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        helper
            .UserDataContext.Balances.Should()
            .ContainSingle(b =>
                b.AccountID == account.ID
                && b.Date == existingBalance.Date
                && b.Amount == balanceCreateRequest.Amount
            );
    }

    [Fact]
    public async Task CreateBalanceAsync_InvalidUserId_ThrowsInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceCreateRequest = new BalanceCreateRequest
        {
            Amount = 1000,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AccountID = Guid.NewGuid(),
        };

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(Guid.NewGuid(), balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithInvalidAccountID_ShouldThrowAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceCreateRequest = new BalanceCreateRequest
        {
            Amount = 1000,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AccountID = Guid.NewGuid(),
        };

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountNotFoundError");
    }
    #endregion

    #region ReadBalancesAsync
    [Fact]
    public async Task ReadBalancesAsync_WhenCalledWithValidData_ShouldReturnBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(3);

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await balanceService.ReadBalancesAsync(helper.demoUser.Id, account.ID);

        // Assert
        result.Should().BeEquivalentTo(balances.Select(b => new BalanceResponse(b)));
    }
    #endregion

    #region UpdateBalanceAsync
    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithValidData_ShouldUpdateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance = balanceFaker.Generate();

        account.Balances.Add(balance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceUpdateRequest = new BalanceUpdateRequest
        {
            ID = balance.ID,
            Amount = balance.Amount + 100,
            Date = balance.Date.AddDays(1),
        };

        // Act
        await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        helper.UserDataContext.Balances.Single().Should().BeEquivalentTo(balanceUpdateRequest);
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithNullValues_ShouldNotUpdateThoseProperties()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance = balanceFaker.Generate();

        account.Balances.Add(balance);

        var oldBalanceAmount = balance.Amount;
        var oldBalanceDate = balance.Date;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceUpdateRequest = new BalanceUpdateRequest
        {
            ID = balance.ID,
            Amount = null,
            Date = null,
        };

        // Act
        await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        var updatedBalance = helper.UserDataContext.Balances.Single();
        updatedBalance.Amount.Should().Be(oldBalanceAmount);
        updatedBalance.Date.Should().Be(oldBalanceDate);
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithInvalidBalanceID_ShouldThrowBalanceNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceUpdateRequest = new BalanceUpdateRequest
        {
            ID = Guid.NewGuid(),
            Amount = 1000,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        // Act
        Func<Task> act = async () =>
            await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceNotFoundError");
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenDuplicateDateExists_ShouldThrowBalanceDuplicateDateError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance1 = balanceFaker.Generate();
        var balance2 = balanceFaker.Generate();
        account.Balances.Add(balance1);
        account.Balances.Add(balance2);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceUpdateRequest = new BalanceUpdateRequest
        {
            ID = balance1.ID,
            Amount = balance1.Amount + 100,
            Date = balance2.Date,
        };

        // Act
        Func<Task> act = async () =>
            await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceDuplicateDateError");
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenDuplicateDateExistsInDifferentAccount_ShouldNotThrowBalanceDuplicateDateError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account1 = accountFaker.Generate();
        var account2 = accountFaker.Generate();

        var balanceFaker1 = new BalanceFaker([account1.ID]);
        var balanceFaker2 = new BalanceFaker([account2.ID]);

        var balance1 = balanceFaker1.Generate();
        var balance2 = balanceFaker2.Generate();

        account1.Balances.Add(balance1);
        account2.Balances.Add(balance2);

        helper.UserDataContext.Accounts.Add(account1);
        helper.UserDataContext.Accounts.Add(account2);
        helper.UserDataContext.SaveChanges();

        // Update balance1 to have the same date as balance2 (different account — should be allowed)
        var balanceUpdateRequest = new BalanceUpdateRequest
        {
            ID = balance1.ID,
            Amount = balance1.Amount,
            Date = balance2.Date,
        };

        // Act
        Func<Task> act = async () =>
            await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        await act.Should().NotThrowAsync();
    }
    #endregion

    #region DeleteBalanceAsync
    [Fact]
    public async Task DeleteBalanceAsync_WhenCalledWithValidData_ShouldDeleteBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance = balanceFaker.Generate();

        account.Balances.Add(balance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await balanceService.DeleteBalanceAsync(helper.demoUser.Id, balance.ID);

        // Assert
        helper.UserDataContext.Balances.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBalanceAsync_WhenCalledWithInvalidBalanceID_ShouldThrowBalanceNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.DeleteBalanceAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceNotFoundError");
    }
    #endregion
}
