using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class SimpleFinOrganizationServiceTests()
{
    [Fact]
    public async Task CreateSimpleFinOrganizationAsync_WhenValidData_ShouldCreateOrganization()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var request = new SimpleFinOrganizationCreateRequest
        {
            Domain = "simplefin.com",
            SimpleFinUrl = "https://simplefin.com/org",
            Name = "SimpleFin",
            Url = "https://simplefin.com",
            SyncID = "sync-123",
        };

        // Act
        await simpleFinOrganizationService.CreateSimpleFinOrganizationAsync(
            helper.demoUser.Id,
            request
        );

        // Assert
        var createdOrganization =
            await helper.UserDataContext.SimpleFinOrganizations.FirstOrDefaultAsync(o =>
                o.Domain == request.Domain && o.UserID == helper.demoUser.Id
            );

        createdOrganization.Should().NotBeNull();
        createdOrganization!.Name.Should().Be(request.Name);
        createdOrganization.SimpleFinUrl.Should().Be(request.SimpleFinUrl);
        createdOrganization.Url.Should().Be(request.Url);
        createdOrganization.SyncID.Should().Be(request.SyncID);
    }

    [Fact]
    public async Task CreateSimpleFinOrganizationAsync_WhenDuplicateDomain_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingOrganization = new SimpleFinOrganization
        {
            Domain = "simplefin.com",
            SimpleFinUrl = "https://simplefin.com/org",
            Name = "SimpleFin",
            Url = "https://simplefin.com",
            SyncID = "sync-123",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.SimpleFinOrganizations.Add(existingOrganization);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new SimpleFinOrganizationCreateRequest
        {
            Domain = "simplefin.com", // Duplicate domain
            SimpleFinUrl = "https://simplefin.com/org2",
            Name = "SimpleFin 2",
            Url = "https://simplefin.com/2",
            SyncID = "sync-456",
        };

        // Act
        Func<Task> act = async () =>
            await simpleFinOrganizationService.CreateSimpleFinOrganizationAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("DuplicateOrganizationCreateError");
    }

    [Fact]
    public async Task ReadSimpleFinOrganizationsAsync_WhenValidData_ShouldReturnOrganizations()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var organization1 = new SimpleFinOrganization
        {
            Domain = "simplefin1.com",
            SimpleFinUrl = "https://simplefin1.com/org",
            Name = "SimpleFin 1",
            Url = "https://simplefin1.com",
            SyncID = "sync-123",
            UserID = helper.demoUser.Id,
        };

        var organization2 = new SimpleFinOrganization
        {
            Domain = "simplefin2.com",
            SimpleFinUrl = "https://simplefin2.com/org",
            Name = "SimpleFin 2",
            Url = "https://simplefin2.com",
            SyncID = "sync-456",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.SimpleFinOrganizations.AddRange(organization1, organization2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var organizations = await simpleFinOrganizationService.ReadSimpleFinOrganizationsAsync(
            helper.demoUser.Id
        );

        // Assert
        organizations.Should().HaveCount(2);
        organizations.Should().ContainSingle(o => o.Domain == organization1.Domain);
        organizations.Should().ContainSingle(o => o.Domain == organization2.Domain);
    }

    [Fact]
    public async Task UpdateSimpleFinOrganizationAsync_WhenValidData_ShouldUpdateOrganization()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingOrganization = new SimpleFinOrganization
        {
            Domain = "simplefin.com",
            SimpleFinUrl = "https://simplefin.com/org",
            Name = "SimpleFin",
            Url = "https://simplefin.com",
            SyncID = "sync-123",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.SimpleFinOrganizations.Add(existingOrganization);
        await helper.UserDataContext.SaveChangesAsync();

        var updateRequest = new SimpleFinOrganizationUpdateRequest
        {
            ID = existingOrganization.ID,
            Domain = "updatedsimplefin.com",
            SimpleFinUrl = "https://updatedsimplefin.com/org",
            Name = "Updated SimpleFin",
            Url = "https://updatedsimplefin.com",
            SyncID = "sync-456",
        };

        // Act
        await simpleFinOrganizationService.UpdateSimpleFinOrganizationAsync(
            helper.demoUser.Id,
            updateRequest
        );

        // Assert
        var updatedOrganization =
            await helper.UserDataContext.SimpleFinOrganizations.FirstOrDefaultAsync(o =>
                o.ID == existingOrganization.ID
            );

        updatedOrganization.Should().NotBeNull();
        updatedOrganization.Domain.Should().Be(updateRequest.Domain);
        updatedOrganization.Name.Should().Be(updateRequest.Name);
        updatedOrganization.SimpleFinUrl.Should().Be(updateRequest.SimpleFinUrl);
        updatedOrganization.Url.Should().Be(updateRequest.Url);
        updatedOrganization.SyncID.Should().Be(updateRequest.SyncID);
    }

    [Fact]
    public async Task UpdateSimpleFinOrganizationAsync_WhenOrganizationNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updateRequest = new SimpleFinOrganizationUpdateRequest
        {
            ID = Guid.NewGuid(), // Non-existent ID
            Domain = "updatedsimplefin.com",
            SimpleFinUrl = "https://updatedsimplefin.com/org",
            Name = "Updated SimpleFin",
            Url = "https://updatedsimplefin.com",
            SyncID = "sync-456",
        };

        // Act
        Func<Task> act = async () =>
            await simpleFinOrganizationService.UpdateSimpleFinOrganizationAsync(
                helper.demoUser.Id,
                updateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinOrganizationUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteSimpleFinOrganizationAsync_WhenValidData_ShouldDeleteOrganization()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingOrganization = new SimpleFinOrganization
        {
            Domain = "simplefin.com",
            SimpleFinUrl = "https://simplefin.com/org",
            Name = "SimpleFin",
            Url = "https://simplefin.com",
            SyncID = "sync-123",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.SimpleFinOrganizations.Add(existingOrganization);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await simpleFinOrganizationService.DeleteSimpleFinOrganizationAsync(
            helper.demoUser.Id,
            existingOrganization.ID
        );

        // Assert
        var deletedOrganization =
            await helper.UserDataContext.SimpleFinOrganizations.FirstOrDefaultAsync(o =>
                o.ID == existingOrganization.ID
            );

        deletedOrganization.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSimpleFinOrganizationAsync_WhenOrganizationNotFound_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinOrganizationService = new SimpleFinOrganizationService(
            Mock.Of<ILogger<ISimpleFinOrganizationService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var nonExistentOrganizationId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await simpleFinOrganizationService.DeleteSimpleFinOrganizationAsync(
                helper.demoUser.Id,
                nonExistentOrganizationId
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinOrganizationDeleteNotFoundError");
    }
}
