using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class UserSettingsServiceTests
{
    private readonly Mock<ILogger<IApplicationUserService>> _loggerMock;
    private readonly Faker<UserSettingsUpdateRequest> _userSettingsUpdateRequestFaker =
        new Faker<UserSettingsUpdateRequest>().RuleFor(
            u => u.Currency,
            f => f.Finance.Currency().Code
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
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var userSettingsFaker = new UserSettingsFaker(helper.demoUser.Id);
        var userSettings = userSettingsFaker.Generate();

        helper.UserDataContext.UserSettings.Add(userSettings);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await userSettingsService.ReadUserSettingsAsync(helper.demoUser.Id);

        // Assert
        result.Should().BeOfType<UserSettingsResponse>();
        result.Currency.Should().Be(userSettings.Currency.ToString());
    }

    [Fact]
    public async Task ReadUserSettingsAsync_WhenUserNotFound_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var readUserSettingsAct = async () =>
            await userSettingsService.ReadUserSettingsAsync(Guid.NewGuid());

        // Assert
        await readUserSettingsAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task ReadUserSettingsAsync_WhenNoSettings_ShouldCreateDefault()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.UserSettings = null;
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await userSettingsService.ReadUserSettingsAsync(helper.demoUser.Id);

        // Assert
        result.Should().BeOfType<UserSettingsResponse>();
        result.Should().BeEquivalentTo(new UserSettingsResponse());
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_WhenValidData_UpdatesUserSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.UserSettings = new UserSettings { UserID = helper.demoUser.Id };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);
        helper.UserDataContext.SaveChanges();

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();
        userSettingsUpdateRequest.Currency = new Faker().Finance.Currency().Code;
        userSettingsUpdateRequest.DisableBuiltInTransactionCategories = true;
        userSettingsUpdateRequest.BudgetWarningThreshold = 50;
        userSettingsUpdateRequest.ForceSyncLookbackMonths = 6;

        // Act
        await userSettingsService.UpdateUserSettingsAsync(
            helper.demoUser.Id,
            userSettingsUpdateRequest
        );

        // Assert
        helper.demoUser.UserSettings.Should().NotBeNull();
        helper.demoUser.UserSettings.Should().BeEquivalentTo(userSettingsUpdateRequest);
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_WhenUserSettingsNotFound_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
            .WithMessage("UserSettingsNotFoundError");
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_WhenInvalidCurrency_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.UserSettings = new UserSettings { UserID = helper.demoUser.Id };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);
        helper.UserDataContext.SaveChanges();

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();
        userSettingsUpdateRequest.Currency = "INVALID";

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
            .WithMessage("InvalidCurrencyCodeError");
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_WhenInvalidBudgetWarningThreshold_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.UserSettings = new UserSettings { UserID = helper.demoUser.Id };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);
        helper.UserDataContext.SaveChanges();

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();
        userSettingsUpdateRequest.BudgetWarningThreshold = 150;

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
            .WithMessage("InvalidBudgetWarningThresholdError");
    }

    [Fact]
    public async Task UpdateUserSettingsAsync_WhenInvalidForceSyncLookbackMonths_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var userSettingsService = new UserSettingsService(
            _loggerMock.Object,
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.UserSettings = new UserSettings { UserID = helper.demoUser.Id };
        helper.UserDataContext.UserSettings.Add(helper.demoUser.UserSettings);
        helper.UserDataContext.SaveChanges();

        var userSettingsUpdateRequest = _userSettingsUpdateRequestFaker.Generate();
        userSettingsUpdateRequest.ForceSyncLookbackMonths = -5;

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
            .WithMessage("InvalidForceSyncLookbackMonthsError");
    }
}
