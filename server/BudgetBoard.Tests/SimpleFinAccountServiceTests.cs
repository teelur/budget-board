using BudgetBoard.Database.Models;
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
public class SimpleFinAccountServiceTests()
{
    #region CreateSimpleFinAccountAsync
    [Fact]
    public async Task CreateSimpleFinAccountAsync_WhenValidData_ShouldCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        await helper.UserDataContext.SaveChangesAsync();

        var createRequest = new SimpleFinAccountCreateRequest
        {
            SyncID = "TestSyncID",
            Name = "Test Account",
            Currency = "USD",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
            OrganizationId = organization.ID,
        };

        // Act
        await simpleFinAccountService.CreateSimpleFinAccountAsync(
            helper.demoUser.Id,
            createRequest
        );

        // Assert
        var createdAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.SyncID == createRequest.SyncID
        );

        createdAccount.Should().NotBeNull();
        createdAccount
            .Should()
            .BeEquivalentTo(createRequest, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task CreateSimpleFinAccountAsync_WhenInvalidOrganizationId_ShouldThrowInvalidOrganizationIDError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var createRequest = new SimpleFinAccountCreateRequest
        {
            SyncID = "TestSyncID",
            Name = "Test Account",
            Currency = "USD",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
            OrganizationId = Guid.NewGuid(),
        };

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.CreateSimpleFinAccountAsync(
                helper.demoUser.Id,
                createRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidOrganizationIDError");
    }
    #endregion

    #region ReadSimpleFinAccountsAsync
    [Fact]
    public async Task ReadSimpleFinAccountsAsync_WhenValidData_ShouldReturnAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account1 = accountFaker.Generate();
        var account2 = accountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.AddRange(account1, account2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var accounts = await simpleFinAccountService.ReadSimpleFinAccountsAsync(helper.demoUser.Id);

        // Assert
        accounts.Should().HaveCount(2);
        accounts.Should().ContainEquivalentOf(new SimpleFinAccountResponse(account1));
        accounts.Should().ContainEquivalentOf(new SimpleFinAccountResponse(account2));
    }
    #endregion

    #region UpdateSimpleFinAccountAsync
    [Fact]
    public async Task UpdateSimpleFinAccountAsync_WhenValidData_ShouldUpdateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var nowProviderMock = new Mock<INowProvider>();
        var fixedNow = new DateTime(2024, 1, 1);
        nowProviderMock.Setup(np => np.UtcNow).Returns(fixedNow);

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = accountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var updateRequest = new SimpleFinAccountUpdateRequest
        {
            ID = account.ID,
            Name = "Updated Account Name",
            Currency = "EUR",
            Balance = 2000.00m,
            BalanceDate = nowProviderMock.Object.UtcNow,
        };

        // Act
        await simpleFinAccountService.UpdateSimpleFinAccountAsync(
            helper.demoUser.Id,
            updateRequest
        );

        // Assert
        var updatedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedAccount.Should().NotBeNull();
        updatedAccount.Name.Should().Be(updateRequest.Name);
        updatedAccount.Currency.Should().Be(updateRequest.Currency);
        updatedAccount.Balance.Should().Be(updateRequest.Balance);
        updatedAccount
            .BalanceDate.Should()
            .Be((int)new DateTimeOffset(updateRequest.BalanceDate).ToUnixTimeSeconds());
    }

    [Fact]
    public async Task UpdateSimpleFinAccountAsync_WhenAccountNotFound_ShouldThrowSimpleFinAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updateRequest = new SimpleFinAccountUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Account Name",
            Currency = "EUR",
            Balance = 2000.00m,
            BalanceDate = DateTime.UtcNow,
        };

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.UpdateSimpleFinAccountAsync(
                helper.demoUser.Id,
                updateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountNotFoundError");
    }
    #endregion

    #region DeleteSimpleFinAccountAsync
    [Fact]
    public async Task DeleteSimpleFinAccountAsync_WhenValidData_ShouldDeleteAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = accountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.DeleteSimpleFinAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        var deletedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSimpleFinAccountAsync_WhenLinkedAccountExists_ShouldResetLinkedAccountSourceToManual()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var linkedAccount = new AccountFaker(helper.demoUser.Id).Generate();
        linkedAccount.Source = AccountSource.SimpleFIN;

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = accountFaker.Generate();
        account.LinkedAccountId = linkedAccount.ID;

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.Add(linkedAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.DeleteSimpleFinAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        var updatedLinkedAccount = helper.UserDataContext.Accounts.First(a =>
            a.ID == linkedAccount.ID
        );
        updatedLinkedAccount.Source.Should().Be(AccountSource.Manual);
    }

    [Fact]
    public async Task DeleteSimpleFinAccountAsync_WhenLinkedAccountDoesNotExist_ShouldDeleteAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organization = new SimpleFinOrganizationFaker(helper.demoUser.Id).Generate();
        var unrelatedAccount = new AccountFaker(helper.demoUser.Id).Generate();
        var account = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID).Generate();
        account.LinkedAccountId = Guid.NewGuid();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.Add(unrelatedAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.DeleteSimpleFinAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper
            .UserDataContext.SimpleFinAccounts.FirstOrDefault(a => a.ID == account.ID)
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task DeleteSimpleFinAccountAsync_WhenAccountNotFound_ShouldThrowSimpleFinAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.DeleteSimpleFinAccountAsync(
                helper.demoUser.Id,
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountNotFoundError");
    }
    #endregion

    #region UpdateLinkedAccountAsync
    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenValidData_ShouldUpdateLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var simpleFinAccountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = simpleFinAccountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(account);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var linkedAccount = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(linkedAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            account.ID,
            linkedAccount.ID
        );

        // Assert
        var updatedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedAccount.Should().NotBeNull();
        updatedAccount.LinkedAccountId.Should().Be(linkedAccount.ID);
        updatedAccount.LastSync.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenUnlinkingAccount_ShouldClearLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organization = new SimpleFinOrganizationFaker(helper.demoUser.Id).Generate();
        var linkedAccount = new AccountFaker(helper.demoUser.Id).Generate();
        linkedAccount.Source = AccountSource.SimpleFIN;
        var simpleFinAccount = new SimpleFinAccountFaker(
            helper.demoUser.Id,
            organization.ID
        ).Generate();
        simpleFinAccount.LinkedAccountId = linkedAccount.ID;

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.Add(linkedAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            simpleFinAccount.ID,
            null
        );

        // Assert
        var updatedSimpleFinAccount = helper.UserDataContext.SimpleFinAccounts.First(a =>
            a.ID == simpleFinAccount.ID
        );
        var updatedLinkedAccount = helper.UserDataContext.Accounts.First(a =>
            a.ID == linkedAccount.ID
        );

        updatedSimpleFinAccount.LinkedAccountId.Should().BeNull();
        updatedSimpleFinAccount.LastSync.Should().BeNull();
        updatedLinkedAccount.Source.Should().Be(AccountSource.Manual);
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenRelinkingToNewAccount_ShouldUpdateBothAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organization = new SimpleFinOrganizationFaker(helper.demoUser.Id).Generate();
        var oldAccount = new AccountFaker(helper.demoUser.Id).Generate();
        oldAccount.Source = AccountSource.SimpleFIN;
        var newAccount = new AccountFaker(helper.demoUser.Id).Generate();
        newAccount.Source = AccountSource.Manual;
        var simpleFinAccount = new SimpleFinAccountFaker(
            helper.demoUser.Id,
            organization.ID
        ).Generate();
        simpleFinAccount.LinkedAccountId = oldAccount.ID;

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.AddRange(oldAccount, newAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            simpleFinAccount.ID,
            newAccount.ID
        );

        // Assert
        var updatedSimpleFinAccount = helper.UserDataContext.SimpleFinAccounts.First(a =>
            a.ID == simpleFinAccount.ID
        );
        var updatedOldAccount = helper.UserDataContext.Accounts.First(a => a.ID == oldAccount.ID);
        var updatedNewAccount = helper.UserDataContext.Accounts.First(a => a.ID == newAccount.ID);

        updatedSimpleFinAccount.LinkedAccountId.Should().Be(newAccount.ID);
        updatedSimpleFinAccount.LastSync.Should().BeNull();
        updatedOldAccount.Source.Should().Be(AccountSource.Manual);
        updatedNewAccount.Source.Should().Be(AccountSource.SimpleFIN);
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenOldLinkedAccountDoesNotExist_ShouldLinkNewAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organization = new SimpleFinOrganizationFaker(helper.demoUser.Id).Generate();
        var newAccount = new AccountFaker(helper.demoUser.Id).Generate();
        newAccount.Source = AccountSource.Manual;
        var simpleFinAccount = new SimpleFinAccountFaker(
            helper.demoUser.Id,
            organization.ID
        ).Generate();
        simpleFinAccount.LinkedAccountId = Guid.NewGuid();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.Add(newAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.UpdateLinkedAccountAsync(
            helper.demoUser.Id,
            simpleFinAccount.ID,
            newAccount.ID
        );

        // Assert
        var updatedSimpleFinAccount = helper.UserDataContext.SimpleFinAccounts.First(a =>
            a.ID == simpleFinAccount.ID
        );
        var updatedNewAccount = helper.UserDataContext.Accounts.First(a => a.ID == newAccount.ID);

        updatedSimpleFinAccount.LinkedAccountId.Should().Be(newAccount.ID);
        updatedSimpleFinAccount.LastSync.Should().BeNull();
        updatedNewAccount.Source.Should().Be(AccountSource.SimpleFIN);
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_WhenAccountNotFound_ShouldThrowSimpleFinAccountNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var linkedAccountId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.UpdateLinkedAccountAsync(
                helper.demoUser.Id,
                Guid.NewGuid(),
                linkedAccountId
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountNotFoundError");
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_LinkedAccountIdIsNotValid_ShouldThrowInvalidLinkedAccountIDError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var simpleFinAccountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var simpleFinAccount = simpleFinAccountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var linkedAccount = accountFaker.Generate();
        linkedAccount.Source = AccountSource.SimpleFIN;

        helper.UserDataContext.Accounts.Add(linkedAccount);
        await helper.UserDataContext.SaveChangesAsync();

        var invalidLinkedAccountId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.UpdateLinkedAccountAsync(
                helper.demoUser.Id,
                simpleFinAccount.ID,
                invalidLinkedAccountId
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidLinkedAccountIDError");
    }
    #endregion

    #region UpdateSimpleFinAccountSyncStartDateAsync
    [Fact]
    public async Task UpdateSimpleFinAccountSyncStartDateAsync_WhenValidData_ShouldUpdateSyncStartDate()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = accountFaker.Generate();

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var newSyncStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));

        // Act
        await simpleFinAccountService.UpdateSimpleFinAccountSyncStartDateAsync(
            helper.demoUser.Id,
            account.ID,
            newSyncStartDate
        );

        // Assert
        var updatedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedAccount.Should().NotBeNull();
        updatedAccount!.SyncStartDate.Should().Be(newSyncStartDate);
    }

    [Fact]
    public async Task UpdateSimpleFinAccountSyncStartDateAsync_WhenClearingSyncStartDate_ShouldSetToNull()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organizationFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var organization = organizationFaker.Generate();

        var accountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, organization.ID);
        var account = accountFaker.Generate();
        account.SyncStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));

        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinAccountService.UpdateSimpleFinAccountSyncStartDateAsync(
            helper.demoUser.Id,
            account.ID,
            null
        );

        // Assert
        var updatedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        updatedAccount.Should().NotBeNull();
        updatedAccount!.SyncStartDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSimpleFinAccountSyncStartDateAsync_WhenAccountNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinAccountService = new SimpleFinAccountService(
            Mock.Of<ILogger<ISimpleFinAccountService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.UpdateSimpleFinAccountSyncStartDateAsync(
                helper.demoUser.Id,
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow)
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountUpdateNotFoundError");
    }
    #endregion
}
