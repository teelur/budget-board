using Bogus;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
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

        // Act
        var result = await applicationUserService.ReadApplicationUserAsync(helper.demoUser.Id);

        // Assert
        result.Should().BeEquivalentTo(new ApplicationUserResponse(helper.demoUser));
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

        // Act
        Func<Task> act = async () =>
            await applicationUserService.ReadApplicationUserAsync(Guid.NewGuid());

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
}
