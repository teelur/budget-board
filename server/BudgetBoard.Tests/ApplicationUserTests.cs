using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class ApplicationUserTests
{
    [Fact]
    public async Task ReadApplicationUserAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var mockUserManager = MockUserManager(helper.demoUser);

        // Act
        var result = await applicationUserService.ReadApplicationUserAsync(
            helper.demoUser.Id,
            mockUserManager.Object
        );

        // Assert
        result.Should().BeEquivalentTo(new ApplicationUserResponse(helper.demoUser, false, false));
    }

    [Fact]
    public async Task ReadApplicationUserAsync_WhenUserHasOidcLogin_ReturnsUserWithOidcLogin()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var mockUserManager = MockUserManager(helper.demoUser);
        mockUserManager
            .Setup(um => um.GetLoginsAsync(helper.demoUser))
            .ReturnsAsync(
                new List<UserLoginInfo> { new UserLoginInfo("oidc", "oidcUserId", "oidc") }
            );

        // Act
        var result = await applicationUserService.ReadApplicationUserAsync(
            helper.demoUser.Id,
            mockUserManager.Object
        );

        // Assert
        result.HasOidcLogin.Should().BeTrue();
        result.HasLocalLogin.Should().BeFalse();
    }

    [Fact]
    public async Task ReadApplicationUserAsync_WhenUserHasLocalLogin_ReturnsUserWithLocalLogin()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var mockUserManager = MockUserManager(helper.demoUser);
        mockUserManager
            .Setup(um => um.GetLoginsAsync(helper.demoUser))
            .ReturnsAsync([new("local", "localUserId", "local")]);

        // Act
        var result = await applicationUserService.ReadApplicationUserAsync(
            helper.demoUser.Id,
            mockUserManager.Object
        );

        // Assert
        result.HasOidcLogin.Should().BeFalse();
        result.HasLocalLogin.Should().BeTrue();
    }

    [Fact]
    public async Task ReadApplicationUserAsync_WhenUserDoesNotExist_ThrowsInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var mockUserManager = MockUserManager(helper.demoUser);

        // Act
        Func<Task> act = async () =>
            await applicationUserService.ReadApplicationUserAsync(
                Guid.NewGuid(),
                mockUserManager.Object
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_WhenUserExists_UpdatesUser()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var userUpdateRequest = new ApplicationUserUpdateRequest
        {
            LastSync = new Faker().Date.Past().ToUniversalTime(),
        };

        // Act
        await applicationUserService.UpdateApplicationUserAsync(
            helper.demoUser.Id,
            userUpdateRequest
        );

        // Assert
        helper.UserDataContext.Users.Single().Should().BeEquivalentTo(userUpdateRequest);
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser user)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        mockUserManager.Setup(um => um.GetLoginsAsync(user)).ReturnsAsync([]);

        return mockUserManager;
    }
}
