using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class UserDataServiceHelperTests
{
    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ShouldReturnUserAndIncludeData()
    {
        // Arrange
        var helper = new TestHelper();
        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var result = await UserDataServiceHelper.GetCurrentUserAsync(
            helper.UserDataContext,
            Mock.Of<ILogger>(),
            TestHelper.CreateMockLocalizer<LogStrings>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            helper.demoUser.Id.ToString(),
            users => users.Include(u => u.Assets)
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(helper.demoUser.Id);
        result.Assets.Should().ContainSingle(a => a.ID == asset.ID);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserDoesNotExist_ShouldThrowInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();

        // Act
        Func<Task> act = async () =>
            await UserDataServiceHelper.GetCurrentUserAsync(
                helper.UserDataContext,
                Mock.Of<ILogger>(),
                TestHelper.CreateMockLocalizer<LogStrings>(),
                TestHelper.CreateMockLocalizer<ResponseStrings>(),
                Guid.NewGuid().ToString(),
                users => users.Include(u => u.Assets)
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenQueryFails_ShouldThrowUserDataRetrievalError()
    {
        // Arrange
        var helper = new TestHelper();
        helper.UserDataContext.Dispose();

        // Act
        Func<Task> act = async () =>
            await UserDataServiceHelper.GetCurrentUserAsync(
                helper.UserDataContext,
                Mock.Of<ILogger>(),
                TestHelper.CreateMockLocalizer<LogStrings>(),
                TestHelper.CreateMockLocalizer<ResponseStrings>(),
                helper.demoUser.Id.ToString(),
                users => users.Include(u => u.Assets)
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("UserDataRetrievalError");
    }
}
