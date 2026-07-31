using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class NetWorthWidgetCategoryServiceTests
{
    #region CreateNetWorthWidgetCategoryAsync
    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenValidData_ShouldCreateCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;

        var line = configuration.Groups.First().Lines.First();

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
            helper.demoUser.Id,
            request
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();
        var createdCategory = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SelectMany(l => l.Categories)
            .FirstOrDefault(c =>
                c.Value == request.Value && c.Type == request.Type && c.Subtype == request.Subtype
            );
        createdCategory.Should().NotBeNull();
        createdCategory.Value.Should().Be(request.Value);
        createdCategory.Type.Should().Be(request.Type);
        createdCategory.Subtype.Should().Be(request.Subtype);
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenLineDoesntExist_ShouldThrowNetWorthWidgetLineNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = widgetSettings.ID,
        };
        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );
        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenWidgetSettingsDontExist_ShouldThrowWidgetSettingsNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = Guid.NewGuid(),
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetSettingsNotFoundError");
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenUserDoesntExist_ShouldThrowInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();
        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = Guid.NewGuid(),
        };
        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                Guid.NewGuid(),
                request
            );
        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenConfigurationIsNull_ShouldThrowWidgetConfigurationNullError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = null,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationNullError");
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenConfigurationIsInvalidJson_ShouldThrowWidgetConfigurationDeserializationError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = "Invalid JSON",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationDeserializationError");
    }

    [Fact]
    public async Task CreateNetWorthWidgetCategoryAsync_WhenConfigurationIsJsonNull_ShouldThrowWidgetConfigurationDeserializationError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = "null",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetCategoryCreateRequest
        {
            Value = "Test Value",
            Type = "Asset",
            Subtype = "Cash",
            LineId = Guid.NewGuid(),
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationDeserializationError");
    }
    #endregion

    #region UpdateNetWorthWidgetCategoryAsync
    [Fact]
    public async Task UpdateNetWorthWidgetCategoryAsync_WhenValidData_ShouldUpdateCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;

        var line = configuration.Groups.First().Lines.First();

        var newCategory = new NetWorthWidgetCategory
        {
            ID = Guid.NewGuid(),
            Value = "Old Value",
            Type = "Liability",
            Subtype = "Credit Card",
        };

        line.Categories.Add(newCategory);

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetCategoryUpdateRequest
        {
            Id = newCategory.ID,
            Value = "Updated Value",
            Type = "Asset",
            Subtype = "Investment",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
            helper.demoUser.Id,
            request
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();
        var updatedCategory = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SelectMany(l => l.Categories)
            .FirstOrDefault(c => c.ID == request.Id);
        updatedCategory.Should().NotBeNull();
        updatedCategory.Value.Should().Be(request.Value);
        updatedCategory.Type.Should().Be(request.Type);
        updatedCategory.Subtype.Should().Be(request.Subtype);
    }

    [Fact]
    public async Task UpdateNetWorthWidgetCategoryAsync_WhenLineNameValueIsValid_ShouldUpdateCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;

        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .Single(l => l.Name == "Investments");
        var targetLine = configuration.Groups.SelectMany(g => g.Lines).First(l => l.ID != line.ID);
        var category = line.Categories.First();

        var request = new NetWorthWidgetCategoryUpdateRequest
        {
            Id = category.ID,
            Value = targetLine.Name,
            Type = "Line",
            Subtype = "Name",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
            helper.demoUser.Id,
            request
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        var updatedCategory = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SelectMany(l => l.Categories)
            .First(c => c.ID == category.ID);

        updatedCategory.Type.Should().Be("Line");
        updatedCategory.Subtype.Should().Be("Name");
        updatedCategory.Value.Should().Be(targetLine.Name);
    }

    [Fact]
    public async Task UpdateNetWorthWidgetCategoryAsync_WhenCategoryDoesntExist_ShouldThrowNetWorthWidgetCategoryNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;
        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .Single(l => l.Name == "Investments");

        var request = new NetWorthWidgetCategoryUpdateRequest
        {
            Id = Guid.NewGuid(),
            Value = "Updated Value",
            Type = "Asset",
            Subtype = "Investment",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetCategoryNotFoundError");
    }

    [Fact]
    public async Task UpdateNetWorthWidgetCategoryAsync_WhenLineNameDoesNotExist_ShouldThrowNetWorthWidgetLineNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;
        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .Single(l => l.Name == "Investments");

        var request = new NetWorthWidgetCategoryUpdateRequest
        {
            Id = line.Categories.First().ID,
            Value = "Updated Value",
            Type = "Line",
            Subtype = "Name",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }

    [Fact]
    public async Task UpdateNetWorthWidgetCategoryAsync_WhenLineNameValueDependsOnThisLine_ShouldThrowNetWorthWidgetCategoryTargetLineDependsOnThisLineError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;

        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .Single(l => l.Name == "Investments");
        var category = line.Categories.First();

        var request = new NetWorthWidgetCategoryUpdateRequest
        {
            Id = category.ID,
            Value = "Total",
            Type = "Line",
            Subtype = "Name",
            LineId = line.ID,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetCategoryTargetLineDependsOnThisLineError");
    }
    #endregion

    #region DeleteNetWorthWidgetCategoryAsync
    [Fact]
    public async Task DeleteNetWorthWidgetCategoryAsync_WhenLineDoesNotExist_ShouldThrowNetWorthWidgetLineNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                widgetSettings.ID,
                Guid.NewGuid(),
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }

    [Fact]
    public async Task DeleteNetWorthWidgetCategoryAsync_WhenConfigurationIsNull_ShouldThrowWidgetConfigurationNullError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = null,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                widgetSettings.ID,
                Guid.NewGuid(),
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationNullError");
    }

    [Fact]
    public async Task DeleteNetWorthWidgetCategoryAsync_WhenConfigurationIsInvalidJson_ShouldThrowWidgetConfigurationDeserializationError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = "Invalid JSON",
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var act = async () =>
            await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
                helper.demoUser.Id,
                widgetSettings.ID,
                Guid.NewGuid(),
                Guid.NewGuid()
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationDeserializationError");
    }

    [Fact]
    public async Task DeleteNetWorthWidgetCategoryAsync_WhenValidData_ShouldDeleteCategory()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetCategoryService = new NetWorthWidgetCategoryService(
            Mock.Of<ILogger<INetWorthWidgetCategoryService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            helper.demoUser.WidgetSettings.First().Configuration!
        )!;
        var line = configuration.Groups.First().Lines.First();

        var newCategory = new NetWorthWidgetCategory
        {
            ID = Guid.NewGuid(),
            Value = "Old Value",
            Type = "Liability",
            Subtype = "Credit Card",
        };

        line.Categories.Add(newCategory);
        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
            helper.demoUser.Id,
            widgetSettings.ID,
            line.ID,
            newCategory.ID
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();
        var deletedCategory = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SelectMany(l => l.Categories)
            .FirstOrDefault(c => c.ID == newCategory.ID);
        deletedCategory.Should().BeNull();
    }
    #endregion
}
