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
    public async Task CreateSimpleFinAccountAsync_WhenInvalidOrganizationId_ShouldThrowException()
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

    [Fact]
    public async Task UpdateAccountAsync_WhenValidData_ShouldUpdateAccount()
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
        await simpleFinAccountService.UpdateAccountAsync(helper.demoUser.Id, updateRequest);

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
    public async Task UpdateAccountAsync_WhenAccountNotFound_ShouldThrowException()
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
            await simpleFinAccountService.UpdateAccountAsync(helper.demoUser.Id, updateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountIDUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenValidData_ShouldDeleteAccount()
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
        await simpleFinAccountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        var deletedAccount = helper.UserDataContext.SimpleFinAccounts.FirstOrDefault(a =>
            a.ID == account.ID
        );

        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenAccountNotFound_ShouldThrowException()
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
            await simpleFinAccountService.DeleteAccountAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinAccountIDDeleteNotFoundError");
    }

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
    public async Task UpdateLinkedAccountAsync_WhenAccountNotFound_ShouldThrowException()
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
            .WithMessage("SimpleFinAccountUpdateNotFoundError");
    }

    [Fact]
    public async Task UpdateLinkedAccountAsync_LinkedAccountIdIsNotValid_ShouldThrowException()
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

        var invalidLinkedAccountId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await simpleFinAccountService.UpdateLinkedAccountAsync(
                helper.demoUser.Id,
                account.ID,
                invalidLinkedAccountId
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidLinkedAccountIDError");
    }
}
