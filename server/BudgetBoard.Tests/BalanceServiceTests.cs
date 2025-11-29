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
    private readonly Faker<BalanceCreateRequest> _balanceCreateRequestFaker =
        new Faker<BalanceCreateRequest>()
            .RuleFor(b => b.Amount, f => f.Finance.Amount())
            .RuleFor(b => b.DateTime, f => f.Date.Past());

    private readonly Faker<BalanceUpdateRequest> _balanceUpdateRequestFaker =
        new Faker<BalanceUpdateRequest>()
            .RuleFor(b => b.Amount, f => f.Finance.Amount())
            .RuleFor(b => b.DateTime, f => f.Date.Past());

    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithValidData_ShouldCreateBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

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
    public async Task CreateBalanceAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceCreateRequest = _balanceCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(Guid.NewGuid(), balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateBalancesAsync_WhenCalledWithInvalidAccountID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceCreateRequest = _balanceCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.CreateBalancesAsync(helper.demoUser.Id, balanceCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceAccountCreateNotFoundError");
    }

    [Fact]
    public async Task ReadBalancesAsync_WhenCalledWithValidData_ShouldReturnBalances()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
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

    [Fact]
    public async Task ReadBalancesAsync_WhenCalledWithInvalidAccountID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.ReadBalancesAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceAccountNotFoundError");
    }

    [Fact]
    public async Task UpdateBalanceAsync_WhenCalledWithValidData_ShouldUpdateBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var balanceUpdateRequest = _balanceUpdateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await balanceService.UpdateBalanceAsync(helper.demoUser.Id, balanceUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteBalanceAsync_WhenCalledWithValidData_ShouldDeleteBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.DeleteBalanceAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceDeleteNotFoundError");
    }

    [Fact]
    public async Task RestoreBalanceAsync_WhenCalledWithValidData_ShouldRestoreBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var balanceFaker = new BalanceFaker([account.ID]);
        var balance = balanceFaker.Generate();
        balance.Deleted = DateTime.UtcNow;

        account.Balances.Add(balance);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await balanceService.RestoreBalanceAsync(helper.demoUser.Id, balance.ID);

        // Assert
        helper.UserDataContext.Balances.Single().Deleted.Should().BeNull();
    }

    [Fact]
    public async Task RestoreBalanceAsync_WhenCalledWithInvalidBalanceID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await balanceService.RestoreBalanceAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("BalanceRestoreNotFoundError");
    }
}
