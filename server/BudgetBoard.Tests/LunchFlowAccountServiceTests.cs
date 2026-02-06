using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class LunchFlowAccountServiceTests()
{
    [Fact]
    public async Task CreateLunchFlowAccountAsync_WhenValidData_ShouldCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var createRequest = new LunchFlowAccountCreateRequest
        {
            SyncID = "TestSyncID",
            Name = "Test LunchFlow Account",
            InstitutionName = "Test Bank",
            InstitutionLogo = "https://example.com/logo.png",
            Provider = "test_provider",
            Currency = "USD",
            Status = "active",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
            LastSync = DateTime.UtcNow,
            LinkedAccountId = null,
        };

        // Act
        await lunchFlowAccountService.CreateLunchFlowAccountAsync(
            helper.demoUser.Id,
            createRequest
        );

        // Assert
        var createdAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.SyncID == createRequest.SyncID
        );

        createdAccount.Should().NotBeNull();
        createdAccount
            .Should()
            .BeEquivalentTo(createRequest, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task CreateLunchFlowAccountAsync_WhenDuplicateSyncID_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var existingAccount = accountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.Add(existingAccount);
        await helper.UserDataContext.SaveChangesAsync();

        var createRequest = new LunchFlowAccountCreateRequest
        {
            SyncID = existingAccount.SyncID,
            Name = "Duplicate Account",
            InstitutionName = "Test Bank",
            InstitutionLogo = "https://example.com/logo.png",
            Provider = "test_provider",
            Currency = "USD",
            Status = "active",
            Balance = 500.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
        };

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.CreateLunchFlowAccountAsync(
                helper.demoUser.Id,
                createRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("DuplicateLunchFlowAccountError");
    }

    [Fact]
    public async Task CreateLunchFlowAccountAsync_WhenInvalidUser_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var createRequest = new LunchFlowAccountCreateRequest
        {
            SyncID = "TestSyncID",
            Name = "Test Account",
            InstitutionName = "Test Bank",
            InstitutionLogo = "https://example.com/logo.png",
            Provider = "test_provider",
            Currency = "USD",
            Status = "active",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
        };

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.CreateLunchFlowAccountAsync(
                Guid.NewGuid(),
                createRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task ReadLunchFlowAccountsAsync_WhenValidData_ShouldReturnAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var account1 = accountFaker.Generate();
        var account2 = accountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.AddRange(account1, account2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var accounts = await lunchFlowAccountService.ReadLunchFlowAccountsAsync(helper.demoUser.Id);

        // Assert
        accounts.Should().HaveCount(2);
        accounts.Should().ContainEquivalentOf(new LunchFlowAccountResponse(account1));
        accounts.Should().ContainEquivalentOf(new LunchFlowAccountResponse(account2));
    }

    [Fact]
    public async Task ReadLunchFlowAccountsAsync_WhenNoAccounts_ShouldReturnEmptyList()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var accounts = await lunchFlowAccountService.ReadLunchFlowAccountsAsync(helper.demoUser.Id);

        // Assert
        accounts.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateLunchFlowAccountAsync_WhenValidData_ShouldUpdateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var updateRequest = new LunchFlowAccountUpdateRequest
        {
            ID = account.ID,
            Name = "Updated Account Name",
            InstitutionName = "Updated Bank",
            InstitutionLogo = "https://example.com/updated-logo.png",
            Provider = "updated_provider",
            Currency = "EUR",
            Status = "inactive",
            Balance = 2000.00m,
            BalanceDate = DateTime.UtcNow.AddDays(-1),
            LastSync = DateTime.UtcNow,
        };

        // Act
        await lunchFlowAccountService.UpdateLunchFlowAccountAsync(
            helper.demoUser.Id,
            updateRequest
        );

        // Assert
        var updatedAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedAccount.Should().NotBeNull();
        updatedAccount.Name.Should().Be(updateRequest.Name);
        updatedAccount.InstitutionName.Should().Be(updateRequest.InstitutionName);
        updatedAccount.InstitutionLogo.Should().Be(updateRequest.InstitutionLogo);
        updatedAccount.Provider.Should().Be(updateRequest.Provider);
        updatedAccount.Currency.Should().Be(updateRequest.Currency);
        updatedAccount.Status.Should().Be(updateRequest.Status);
        updatedAccount.Balance.Should().Be(updateRequest.Balance);
    }

    [Fact]
    public async Task UpdateLunchFlowAccountAsync_WhenAccountNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updateRequest = new LunchFlowAccountUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Non-existent Account",
            InstitutionName = "Test Bank",
            InstitutionLogo = "https://example.com/logo.png",
            Provider = "test_provider",
            Currency = "USD",
            Status = "active",
            Balance = 1000.00m,
            BalanceDate = DateTime.UtcNow,
        };

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.UpdateLunchFlowAccountAsync(
                helper.demoUser.Id,
                updateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("LunchFlowAccountNotFoundError");
    }

    [Fact]
    public async Task DeleteLunchFlowAccountAsync_WhenValidData_ShouldDeleteAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await lunchFlowAccountService.DeleteLunchFlowAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        var deletedAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task DeleteLunchFlowAccountAsync_WhenAccountNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.DeleteLunchFlowAccountAsync(
                helper.demoUser.Id,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("LunchFlowAccountNotFoundError");
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenLinkingValidAccount_ShouldUpdateLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await lunchFlowAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            lunchFlowAccount.ID,
            account.ID
        );

        // Assert
        var updatedLunchFlowAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == lunchFlowAccount.ID
        );
        var updatedAccount = helper.UserDataContext.Accounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedLunchFlowAccount.Should().NotBeNull();
        updatedLunchFlowAccount.LinkedAccountId.Should().Be(account.ID);
        updatedLunchFlowAccount.LastSync.Should().BeNull();
        updatedAccount.Should().NotBeNull();
        updatedAccount.Source.Should().Be(AccountSource.LunchFlow);
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenUnlinkingAccount_ShouldClearLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.LunchFlow;

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await lunchFlowAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            lunchFlowAccount.ID,
            null
        );

        // Assert
        var updatedLunchFlowAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == lunchFlowAccount.ID
        );
        var updatedAccount = helper.UserDataContext.Accounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedLunchFlowAccount.Should().NotBeNull();
        updatedLunchFlowAccount!.LinkedAccountId.Should().BeNull();
        updatedLunchFlowAccount.LastSync.Should().BeNull();
        updatedAccount.Should().NotBeNull();
        updatedAccount!.Source.Should().Be(AccountSource.Manual);
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenLunchFlowAccountNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.UpdateLinkedAccountAsync(
                helper.demoUser.Id,
                Guid.NewGuid(),
                account.ID
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("LunchFlowAccountNotFoundError");
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenLinkedAccountNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();

        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
            await lunchFlowAccountService.UpdateLinkedAccountAsync(
                helper.demoUser.Id,
                lunchFlowAccount.ID,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidLinkedAccountIDError");
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenRelinkingToNewAccount_ShouldUpdateBothAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var lunchFlowAccountService = new LunchFlowAccountService(
            Mock.Of<ILogger<ILunchFlowAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var oldAccount = accountFaker.Generate();
        oldAccount.Source = AccountSource.LunchFlow;
        var newAccount = accountFaker.Generate();
        newAccount.Source = AccountSource.Manual;

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = oldAccount.ID;

        helper.UserDataContext.Accounts.AddRange(oldAccount, newAccount);
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await lunchFlowAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            lunchFlowAccount.ID,
            newAccount.ID
        );

        // Assert
        var updatedLunchFlowAccount = helper.UserDataContext.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == lunchFlowAccount.ID
        );
        var updatedOldAccount = helper.UserDataContext.Accounts.FirstOrDefault(a =>
            a.ID == oldAccount.ID
        );
        var updatedNewAccount = helper.UserDataContext.Accounts.FirstOrDefault(a =>
            a.ID == newAccount.ID
        );

        updatedLunchFlowAccount.Should().NotBeNull();
        updatedLunchFlowAccount!.LinkedAccountId.Should().Be(newAccount.ID);
        updatedLunchFlowAccount.LastSync.Should().BeNull();

        updatedOldAccount.Should().NotBeNull();
        updatedOldAccount!.Source.Should().Be(AccountSource.Manual);

        updatedNewAccount.Should().NotBeNull();
        updatedNewAccount!.Source.Should().Be(AccountSource.LunchFlow);
    }
}
