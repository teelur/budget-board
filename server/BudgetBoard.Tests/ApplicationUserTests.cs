using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
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
            helper.UserDataContext
        );

        var mockUserManager = MockUserManager(helper.demoUser);

        // Act
        var result = await applicationUserService.ReadApplicationUserAsync(
            helper.demoUser.Id,
            mockUserManager.Object
        );

        // Assert
        result.Should().BeEquivalentTo(new ApplicationUserResponse(helper.demoUser, false));
    }

    [Fact]
    public async Task ReadApplicationUserAsync_WhenUserDoesNotExist_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext
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
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_WhenUserExists_UpdatesUser()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext
        );
        var userUpdateRequest = new ApplicationUserUpdateRequest { LastSync = fakeDate };

        // Act
        await applicationUserService.UpdateApplicationUserAsync(
            helper.demoUser.Id,
            userUpdateRequest
        );

        // Assert
        helper.UserDataContext.Users.Single().Should().BeEquivalentTo(userUpdateRequest);
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_WhenUserDoesNotExist_ThrowsError()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var helper = new TestHelper();
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext
        );

        var userUpdateRequest = new ApplicationUserUpdateRequest { LastSync = fakeDate };

        // Act
        Func<Task> act = async () =>
            await applicationUserService.UpdateApplicationUserAsync(
                Guid.NewGuid(),
                userUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
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

        mockUserManager
            .Setup(um => um.GetLoginsAsync(user))
            .ReturnsAsync(new List<UserLoginInfo>());

        return mockUserManager;
    }
}
