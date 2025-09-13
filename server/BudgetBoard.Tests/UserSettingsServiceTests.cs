using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class UserSettingsServiceTests
{
    private readonly Mock<ILogger<IApplicationUserService>> _loggerMock;
    private readonly Faker<UserSettingsUpdateRequest> _userSettingsUpdateRequestFaker =
        new Faker<UserSettingsUpdateRequest>().RuleFor(
            s => s.Currency,
            f => f.Random.Enum<Currency>().ToString()
        );

    public UserSettingsServiceTests()
    {
        _loggerMock = new Mock<ILogger<IApplicationUserService>>();
    }

    [Fact]
    public async Task ReadUserSettingsAsync_ReturnsUserSettingsResponse_WhenUserExists()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        var userSettingsFaker = new UserSettingsFaker();
        var userSettings = userSettingsFaker.Generate();
        userSettings.UserID = helper.demoUser.Id;

        helper.demoUser.UserSettings = userSettings;

        helper.UserDataContext.UserSettings.Add(userSettings);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await userSettingsService.ReadUserSettingsAsync(helper.demoUser.Id);

        // Assert
        result.Should().BeOfType<UserSettingsResponse>();
        result.Currency.Should().Be(userSettings.Currency.ToString());
    }

    [Fact]
    public async Task ReadUserSettingsAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        var userGuid = Guid.NewGuid();

        // Act
        var readUserSettingsAct = async () =>
            await userSettingsService.ReadUserSettingsAsync(userGuid);

        // Assert
        await readUserSettingsAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task ReadUserSettingsAsync_WhenNoSettings_ShouldCreateDefault()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        helper.demoUser.UserSettings = null;

        // Act
        var result = await userSettingsService.ReadUserSettingsAsync(helper.demoUser.Id);

        // Assert
        result.Should().BeOfType<UserSettingsResponse>();
        result.Currency.Should().Be(Currency.USD.ToString()); // Default currency
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_UpdatesCurrency_WhenUserExists()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        helper.demoUser.UserSettings ??= new UserSettings { UserID = helper.demoUser.Id };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);
        helper.UserDataContext.SaveChanges();

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();

        // Act
        await userSettingsService.UpdateUserSettingsAsync(
            helper.demoUser.Id,
            userSettingsUpdateRequest
        );

        // Assert
        helper.demoUser.UserSettings.Currency.Should().Be(userSettingsUpdateRequest.Currency);
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();

        // Act
        var updateUserSettingsAct = async () =>
            await userSettingsService.UpdateUserSettingsAsync(
                new Guid(),
                userSettingsUpdateRequest
            );

        // Assert
        await updateUserSettingsAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_Throws_WhenUserSettingsNotFound()
    {
        // Arrange
        var helper = new TestHelper();
        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext
        );

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();
        helper.demoUser.UserSettings = null;

        // Act
        var updateUserSettingsAct = async () =>
            await userSettingsService.UpdateUserSettingsAsync(
                helper.demoUser.Id,
                userSettingsUpdateRequest
            );

        // Assert
        await updateUserSettingsAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("User settings not found.");
    }
}
