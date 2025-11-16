using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class BalanceServiceTests
{
    private readonly Faker<BalanceCreateRequest> _balanceCreateRequestFaker =
        new Faker<BalanceCreateRequest>()
            .RuleFor(b => b.Amount, f => f.Finance.Amount())
            .RuleFor(b => b.DateTime, f => f.Date.Past());

    private readonly Faker<BalanceUpdateRequest> _balanceUpdateRequestFaker =
        new Faker<BalanceUpdateRequest>()
            .RuleFor(b => b.Amount, f => f.Finance.Amount())
            .RuleFor(b => b.DateTime, f => f.Date.Past());

    [Fact]
    public async Task CreateBalanceAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var balanceCreateRequest = _balanceCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(Guid.NewGuid(), balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithValidData_ShouldCreateBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceCreateRequest = _balanceCreateRequestFaker.Generate();
        balanceCreateRequest.AccountID = account.ID;

        // Act
        await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        helper.UserDataContext.Balances.Should().ContainSingle();
        helper.UserDataContext.Balances.Single().Should().BeEquivalentTo(balanceCreateRequest);
    }

    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithInvalidAccountID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var balanceCreateRequest = _balanceCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The account you are trying to add a balance to does not exist.");
    }

    [Fact]
    public async Task ReadBalancesAsync_WhenCalledWithValidData_ShouldReturnBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var balanceFaker = new BalanceFaker();
        balanceFaker.AccountIds.Add(account.ID);
        var balances = balanceFaker.Generate(3);

        account.Balances = balances;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await balanceService.ReadBalancesAsync(helper.demoUser.Id, account.ID);

        // Assert
        result.Should().BeEquivalentTo(balances.Select(b => new BalanceResponse(b)));
    }

    [Fact]
    public async Task ReadBalancesAsync_WhenCalledWithInvalidAccountID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.ReadBalancesAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The account you are trying to read a balance from does not exist.");
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithValidData_ShouldUpdateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var balanceFaker = new BalanceFaker();
        balanceFaker.AccountIds.Add(account.ID);
        var balance = balanceFaker.Generate();

        account.Balances.Add(balance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var balanceUpdateRequest = _balanceUpdateRequestFaker.Generate();
        balanceUpdateRequest.ID = balance.ID;
        balanceUpdateRequest.AccountID = account.ID;

        // Act
        await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        helper.UserDataContext.Balances.Single().Should().BeEquivalentTo(balanceUpdateRequest);
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithInvalidBalanceID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var balanceUpdateRequest = _balanceUpdateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The balance you are trying to update does not exist.");
    }

    [Fact]
    public async Task DeleteBalanceAsync_WhenCalledWithValidData_ShouldDeleteBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        var balanceFaker = new BalanceFaker();
        balanceFaker.AccountIds.Add(account.ID);
        var balance = balanceFaker.Generate();

        account.Balances.Add(balance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await balanceService.DeleteBalanceAsync(helper.demoUser.Id, balance.ID);

        // Assert
        helper.UserDataContext.Balances.Single().Deleted.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteBalanceAsync_WhenCalledWithInvalidBalanceID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.DeleteBalanceAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The balance you are trying to delete does not exist.");
    }
}
