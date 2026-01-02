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
}
