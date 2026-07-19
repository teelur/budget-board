using Bogus;
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
public class ValueServiceTests
{
    #region CreateValueAsync
    [Fact]
    public async Task CreateValueAsync_WhenValidData_ShouldCreateValue()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var valueCreateRequest = new ValueCreateRequest
        {
            Amount = 1200,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AssetID = asset.ID,
        };

        // Act
        await valueService.CreateValueAsync(helper.demoUser.Id, valueCreateRequest);

        // Assert
        helper
            .UserDataContext.Values.Should()
            .ContainSingle(v =>
                v.Amount == valueCreateRequest.Amount
                && v.Date == valueCreateRequest.Date
                && v.AssetID == valueCreateRequest.AssetID
            );
    }

    [Fact]
    public async Task CreateValueAsync_WhenUserInvalid_ShouldThrowInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var valueCreateRequest = new ValueCreateRequest
        {
            Amount = 1200,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AssetID = Guid.Empty,
        };

        // Act
        Func<Task> act = async () =>
            await valueService.CreateValueAsync(Guid.NewGuid(), valueCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateValueAsync_WhenAssetDoesNotExist_ShouldThrowValueAssetNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var valueCreateRequest = new ValueCreateRequest
        {
            Amount = 1200,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            AssetID = Guid.NewGuid(),
        };

        // Act
        Func<Task> act = async () =>
            await valueService.CreateValueAsync(helper.demoUser.Id, valueCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueAssetNotFoundError");
    }

    [Fact]
    public async Task CreateValueAsync_WhenValueExistsForSameDate_ShouldUpdateExistingValue()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var existingValue = new ValueFaker().Generate();
        existingValue.AssetID = asset.ID;
        asset.Values.Add(existingValue);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var valueCreateRequest = new ValueCreateRequest
        {
            Amount = existingValue.Amount + 100,
            Date = existingValue.Date,
            AssetID = asset.ID,
        };

        // Act
        await valueService.CreateValueAsync(helper.demoUser.Id, valueCreateRequest);

        // Assert
        helper
            .UserDataContext.Values.Should()
            .ContainSingle(v =>
                v.AssetID == asset.ID
                && v.Date == existingValue.Date
                && v.Amount == valueCreateRequest.Amount
            );
    }
    #endregion

    #region ReadValuesAsync
    [Fact]
    public async Task ReadValuesAsync_WhenValuesExist_ShouldReturnValues()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
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
        foreach (var returnedValue in returnedValues)
        {
            values
                .Should()
                .ContainSingle(v =>
                    v.ID == returnedValue.ID
                    && v.AssetID == returnedValue.AssetID
                    && v.Date == returnedValue.Date
                    && v.Amount == returnedValue.Amount
                );
        }
    }

    [Fact]
    public async Task ReadValuesAsync_WhenInvalidAssetId_ShouldThrowValueAssetNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
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
    #endregion

    #region UpdateValueAsync
    [Fact]
    public async Task UpdateValueAsync_WhenValueExists_ShouldUpdateValue()
    {
        // Arrange
        var helper = new TestHelper();

        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
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
            Date = value.Date.AddDays(1),
        };

        // Act
        await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.ID == value.ID && v.Amount == editedValue.Amount && v.Date == editedValue.Date
            );
    }

    [Fact]
    public async Task UpdateValueAsync_WhenValueDoesNotExist_ShouldThrowValueNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var editedValue = new ValueUpdateRequest
        {
            ID = Guid.NewGuid(),
            Amount = 500,
            Date = DateOnly.FromDateTime(DateTime.Now),
        };

        // Act
        Func<Task> act = async () =>
            await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueNotFoundError");
    }

    [Fact]
    public async Task UpdateValueAsync_WhenDuplicateDateExists_ShouldThrowValueDuplicateDateError()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value1 = new ValueFaker().Generate();
        value1.AssetID = asset.ID;
        var value2 = new ValueFaker().Generate();
        value2.AssetID = asset.ID;
        asset.Values.Add(value1);
        asset.Values.Add(value2);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var editedValue = new ValueUpdateRequest
        {
            ID = value1.ID,
            Amount = value1.Amount + 100,
            Date = value2.Date,
        };

        // Act
        Func<Task> act = async () =>
            await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("ValueDuplicateDateError");
    }

    [Fact]
    public async Task UpdateValueAsync_WhenDuplicateDateExistsInDifferentAsset_ShouldNotThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset1 = new AssetFaker(helper.demoUser.Id).Generate();
        var asset2 = new AssetFaker(helper.demoUser.Id).Generate();

        var value1 = new ValueFaker().Generate();
        value1.AssetID = asset1.ID;
        var value2 = new ValueFaker().Generate();
        value2.AssetID = asset2.ID;

        asset1.Values.Add(value1);
        asset2.Values.Add(value2);

        helper.UserDataContext.Assets.Add(asset1);
        helper.UserDataContext.Assets.Add(asset2);
        await helper.UserDataContext.SaveChangesAsync();

        // Update value1 to have the same date as value2 (different asset — should be allowed)
        var editedValue = new ValueUpdateRequest
        {
            ID = value1.ID,
            Amount = value1.Amount,
            Date = value2.Date,
        };

        // Act
        Func<Task> act = async () =>
            await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateValueAsync_WhenOnlyAmountProvided_ShouldUpdateAmountAndKeepDate()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value1 = new ValueFaker().Generate();
        value1.AssetID = asset.ID;
        var value2 = new ValueFaker().Generate();
        value2.AssetID = asset.ID;

        asset.Values.Add(value1);
        asset.Values.Add(value2);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var originalDate = value1.Date;
        var editedValue = new ValueUpdateRequest
        {
            ID = value1.ID,
            Amount = value1.Amount + 100,
            Date = null,
        };

        // Act
        Func<Task> act = async () =>
            await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        await act.Should().NotThrowAsync();

        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.ID == value1.ID && v.Amount == editedValue.Amount && v.Date == originalDate
            );
    }

    [Fact]
    public async Task UpdateValueAsync_WhenOnlyDateProvided_ShouldUpdateDateAndKeepAmount()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value = new ValueFaker().Generate();
        value.AssetID = asset.ID;
        asset.Values.Add(value);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var originalAmount = value.Amount;
        var editedValue = new ValueUpdateRequest
        {
            ID = value.ID,
            Amount = null,
            Date = value.Date.AddDays(1),
        };

        // Act
        await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.ID == value.ID && v.Amount == originalAmount && v.Date == editedValue.Date
            );
    }

    [Fact]
    public async Task UpdateValueAsync_WhenAmountAndDateAreNull_ShouldNotChangeValue()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var asset = new AssetFaker(helper.demoUser.Id).Generate();

        var value = new ValueFaker().Generate();
        value.AssetID = asset.ID;
        asset.Values.Add(value);

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        var originalAmount = value.Amount;
        var originalDate = value.Date;

        var editedValue = new ValueUpdateRequest
        {
            ID = value.ID,
            Amount = null,
            Date = null,
        };

        // Act
        await valueService.UpdateValueAsync(helper.demoUser.Id, editedValue);

        // Assert
        helper
            .demoUser.Assets.SelectMany(a => a.Values)
            .Should()
            .ContainSingle(v =>
                v.ID == value.ID && v.Amount == originalAmount && v.Date == originalDate
            );
    }
    #endregion

    #region DeleteValueAsync
    [Fact]
    public async Task DeleteValueAsync_WhenValueExists_ShouldDeleteValue()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
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
        helper.UserDataContext.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteValueAsync_WhenValueDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var valueService = new ValueService(
            Mock.Of<ILogger<IValueService>>(),
            helper.UserDataContext,
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
    #endregion
}
