using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class ValueServiceTests
{
    private readonly Faker<ValueCreateRequest> _valueCreateRequestFaker =
        new Faker<ValueCreateRequest>()
            .RuleFor(v => v.Amount, f => f.Finance.Amount(-10000, 10000))
            .RuleFor(v => v.DateTime, f => f.Date.Past())
            .RuleFor(v => v.AssetID, f => Guid.Empty);

    [Fact]
    public async Task CreateValueAsync_WhenValidData_ShouldCreateValue()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var valueCreateRequest = _valueCreateRequestFaker.Generate();
        valueCreateRequest.AssetID = asset.ID;

        // Act
        await valueService.CreateValueAsync(helper.demoUser.Id, valueCreateRequest);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.Amount == valueCreateRequest.Amount
                && v.DateTime == valueCreateRequest.DateTime
                && v.AssetID == valueCreateRequest.AssetID
            );
    }

    [Fact]
    public async Task CreateValueAsync_WhenUserInvalid_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var valueCreateRequest = _valueCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await valueService.CreateValueAsync(Guid.NewGuid(), valueCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateValueAsync_WhenAssetDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var valueCreateRequest = _valueCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await valueService.CreateValueAsync(helper.demoUser.Id, valueCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueCreateAssetNotFoundError");
    }

    [Fact]
    public async Task ReadValuesAsync_WhenValuesExist_ShouldReturnValues()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var values = new ValueFaker().Generate(3);
        values.ForEach(v => v.AssetID = asset.ID);
        asset.Values = values;

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var returnedValues = await valueService.ReadValuesAsync(helper.demoUser.Id, asset.ID);

        // Assert
        returnedValues.Should().HaveCount(3);
    }

    [Fact]
    public async Task ReadValuesAsync_WhenInvalidAssetId_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var values = new ValueFaker().Generate(3);
        values.ForEach(v => v.AssetID = asset.ID);
        asset.Values = values;

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
            await valueService.ReadValuesAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueAssetNotFoundError");
    }

    [Fact]
    public async Task UpdateValueAsync_WhenValueExists_ShouldUpdateValue()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value = new ValueFaker().Generate();
        value.AssetID = asset.ID;
        asset.Values.Add(value);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var editedValue = new ValueUpdateRequest
        {
            ID = value.ID,
            Amount = value.Amount + 100,
            DateTime = value.DateTime.AddDays(1),
        };

        // Act
        await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.ID == value.ID
                && v.Amount == editedValue.Amount
                && v.DateTime == editedValue.DateTime
            );
    }

    [Fact]
    public async Task UpdateValueAsync_WhenValueDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var editedValue = new ValueUpdateRequest
        {
            ID = Guid.NewGuid(),
            Amount = 500,
            DateTime = DateTime.UtcNow,
        };

        // Act
        Func<Task> act = async () =>
            await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteValueAsync_WhenValueExists_ShouldMarkValueAsDeleted()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value = new ValueFaker().Generate();
        value.AssetID = asset.ID;
        asset.Values.Add(value);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await valueService.DeleteValueAsync(helper.demoUser.Id, value.ID);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v => v.ID == value.ID && v.Deleted.HasValue);
    }

    [Fact]
    public async Task DeleteValueAsync_WhenValueDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await valueService.DeleteValueAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueDeleteNotFoundError");
    }

    [Fact]
    public async Task RestoreValueAsync_WhenValueExists_ShouldUnmarkValueAsDeleted()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value = new ValueFaker().RuleFor(v => v.Deleted, f => f.Date.Past()).Generate();
        value.AssetID = asset.ID;
        asset.Values.Add(value);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await valueService.RestoreValueAsync(helper.demoUser.Id, value.ID);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v => v.ID == value.ID && !v.Deleted.HasValue);
    }

    [Fact]
    public async Task RestoreValueAsync_WhenValueDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await valueService.RestoreValueAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueRestoreNotFoundError");
    }
}
