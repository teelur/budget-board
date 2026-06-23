using Bogus;
using BudgetBoard.Database.Models;
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
public class AssetTypeServiceTests
{
    private readonly Faker<AssetTypeCreateRequest> _assetTypeCreateRequestFaker =
        new Faker<AssetTypeCreateRequest>()
            .RuleFor(a => a.Value, f => f.Random.String(20))
            .RuleFor(a => a.Parent, f => f.Random.String(20));

    #region CreateAssetTypeAsync
    [Fact]
    public async Task CreateAssetTypeAsync_WhenCalledWithValidData_ShouldCreateAssetType()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();

        helper.UserDataContext.AssetTypes.Add(parentAssetType);
        helper.UserDataContext.SaveChanges();

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();
        assetTypeCreateRequest.Parent = parentAssetType.Value;

        // Act
        await assetTypeService.CreateAssetTypeAsync(helper.demoUser.Id, assetTypeCreateRequest);

        // Assert
        helper.UserDataContext.AssetTypes.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAssetTypeAsync_WhenInvalidUserId_ThrowsInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await assetTypeService.CreateAssetTypeAsync(Guid.NewGuid(), assetTypeCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateAssetTypeAsync_WhenCreatingEmptyName_ShouldThrowEmptyNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();
        assetTypeCreateRequest.Value = string.Empty;

        // Act
        Func<Task> act = async () =>
            await assetTypeService.CreateAssetTypeAsync(helper.demoUser.Id, assetTypeCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeEmptyNameError");
    }

    [Fact]
    public async Task CreateAssetTypeAsync_WhenCreatingDuplicate_ShouldThrowDuplicateNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();

        helper.UserDataContext.AssetTypes.Add(
            new AssetType
            {
                Value = assetTypeCreateRequest.Value,
                Parent = assetTypeCreateRequest.Parent,
                UserID = helper.demoUser.Id,
            }
        );
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await assetTypeService.CreateAssetTypeAsync(helper.demoUser.Id, assetTypeCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeDuplicateNameError");
    }

    [Fact]
    public async Task CreateAssetTypeAsync_WhenParentSameAsValue_ShouldThrowSameNameAsParentError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();
        assetTypeCreateRequest.Parent = assetTypeCreateRequest.Value;

        // Act
        Func<Task> act = async () =>
            await assetTypeService.CreateAssetTypeAsync(helper.demoUser.Id, assetTypeCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeSameNameAsParentError");
    }

    [Fact]
    public async Task CreateAssetTypeAsync_WhenParentDoesNotExist_ShouldThrowAssetTypeParentNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeCreateRequest = _assetTypeCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await assetTypeService.CreateAssetTypeAsync(helper.demoUser.Id, assetTypeCreateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeParentNotFoundError");
    }
    #endregion

    #region ReadAssetTypesAsync
    [Fact]
    public async Task ReadAssetTypesAsync_WhenCalledWithValidData_ShouldReturnAssetTypes()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await assetTypeService.ReadAssetTypesAsync(helper.demoUser.Id);

        // Assert
        result.Select(r => r.ID).Should().Contain(assetTypes.Select(a => a.ID));
    }

    [Fact]
    public async Task ReadAssetTypesAsync_WhenBuiltInTypesDisabled_ShouldNotReturnBuiltInTypes()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.UserDataContext.UserSettings.Add(
            new UserSettings { UserID = helper.demoUser.Id, DisableBuiltInAssetTypes = true }
        );
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await assetTypeService.ReadAssetTypesAsync(helper.demoUser.Id);

        // Assert
        result
            .Should()
            .NotContain(r => AssetTypeConstants.DefaultAssetTypes.Any(dat => dat.Value == r.Value));
    }
    #endregion

    #region UpdateAssetTypeAsync
    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithValidData_ShouldUpdateAssetType()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);
        assetTypes.ForEach(at => at.Parent = string.Empty);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetTypes.First().ID,
            Parent = assetTypes.Last().Value,
            Value = "UpdatedValue",
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        var updatedAssetType = helper.UserDataContext.AssetTypes.First(at =>
            at.ID == assetTypeUpdateRequest.ID
        );
        updatedAssetType.Value.Should().Be(assetTypeUpdateRequest.Value);
        updatedAssetType.Parent.Should().Be(assetTypeUpdateRequest.Parent);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenOmitProperties_ShouldNotUpdateThem()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);
        assetTypes.ForEach(at => at.Parent = string.Empty);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var oldAssetType = new AssetTypeResponse(assetTypes.First());
        var assetTypeUpdateRequest = new AssetTypeUpdateRequest { ID = assetTypes.First().ID };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        var updatedAssetType = helper.UserDataContext.AssetTypes.First(at =>
            at.ID == assetTypeUpdateRequest.ID
        );
        updatedAssetType.Value.Should().Be(oldAssetType.Value);
        updatedAssetType.Parent.Should().Be(oldAssetType.Parent);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithInvalidAssetTypeID_ShouldThrowAssetTypeNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = Guid.NewGuid(),
            Value = "test",
        };

        // Act
        Func<Task> act = async () =>
            await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeNotFoundError");
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithEmptyValue_ShouldThrowAssetTypeEmptyNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetTypes.First().ID,
            Parent = assetTypes.First().Parent,
            Value = string.Empty,
        };

        // Act
        Func<Task> act = async () =>
            await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeEmptyNameError");
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithDuplicateName_ShouldThrowAssetTypeDuplicateNameError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetTypes.First().ID,
            Parent = assetTypes.First().Parent,
            Value = assetTypes.Last().Value,
        };

        // Act
        Func<Task> act = async () =>
            await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeDuplicateNameError");
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithSameNameAsParent_ShouldThrowAssetTypeSameNameAsParentError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetTypes.First().ID,
            Parent = assetTypes.First().Value,
            Value = assetTypes.First().Value,
        };

        // Act
        Func<Task> act = async () =>
            await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeSameNameAsParentError");
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenCalledWithParentThatDoesNotExist_ShouldThrowAssetTypeParentNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetTypes.First().ID,
            Parent = "NonExistentParent",
            Value = assetTypes.First().Value,
        };

        // Act
        Func<Task> act = async () =>
            await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeParentNotFoundError");
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenValueChanges_ShouldUpdateAssetsUsingThatType()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetType = assetTypeFaker.Generate();
        assetType.Parent = string.Empty;

        helper.UserDataContext.AssetTypes.Add(assetType);

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();
        asset.Type = assetType.Value;

        helper.UserDataContext.Assets.Add(asset);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = assetType.ID,
            Parent = assetType.Parent,
            Value = "UpdatedAssetTypeValue",
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        asset.Type.Should().Be(assetTypeUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenParentAlreadyHasParent_ShouldNotResetChildrenParent()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();
        parentAssetType.Parent = "ExistingParent";

        var childAssetType = assetTypeFaker.Generate();
        childAssetType.Parent = parentAssetType.Value;

        var newParentType = assetTypeFaker.Generate();
        newParentType.Parent = string.Empty;

        helper.UserDataContext.AssetTypes.AddRange(
            [parentAssetType, childAssetType, newParentType]
        );
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = parentAssetType.ID,
            Parent = newParentType.Value,
            Value = parentAssetType.Value,
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        helper
            .UserDataContext.AssetTypes.Single(at => at.ID == childAssetType.ID)
            .Parent.Should()
            .Be(parentAssetType.Value);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenUpdateParentValue_ShouldUpdateChildrenParent()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();
        parentAssetType.Parent = string.Empty;

        var childAssetType = assetTypeFaker.Generate();
        childAssetType.Parent = parentAssetType.Value;

        helper.UserDataContext.AssetTypes.AddRange([parentAssetType, childAssetType]);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = parentAssetType.ID,
            Parent = parentAssetType.Parent,
            Value = "UpdatedParentValue",
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        helper
            .UserDataContext.AssetTypes.Single(at => at.ID == childAssetType.ID)
            .Parent.Should()
            .Be(assetTypeUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenParentSpecifiedButDoesNotUpdate_ShouldNotUpdateChildrenParent()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();
        parentAssetType.Parent = string.Empty;

        var childAssetType = assetTypeFaker.Generate();
        childAssetType.Parent = parentAssetType.Value;

        helper.UserDataContext.AssetTypes.AddRange([parentAssetType, childAssetType]);
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = parentAssetType.ID,
            Parent = parentAssetType.Parent,
            Value = parentAssetType.Value,
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        helper
            .UserDataContext.AssetTypes.Single(at => at.ID == childAssetType.ID)
            .Parent.Should()
            .Be(parentAssetType.Value);
    }

    [Fact]
    public async Task UpdateAssetTypeAsync_WhenParentChangesFromEmptyToNonEmpty_ShouldResetChildrenParent()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();
        parentAssetType.Parent = string.Empty;

        var childAssetType = assetTypeFaker.Generate();
        childAssetType.Parent = parentAssetType.Value;

        var newParentType = assetTypeFaker.Generate();
        newParentType.Parent = string.Empty;

        helper.UserDataContext.AssetTypes.AddRange(
            [parentAssetType, childAssetType, newParentType]
        );
        helper.UserDataContext.SaveChanges();

        var assetTypeUpdateRequest = new AssetTypeUpdateRequest
        {
            ID = parentAssetType.ID,
            Parent = newParentType.Value,
        };

        // Act
        await assetTypeService.UpdateAssetTypeAsync(helper.demoUser.Id, assetTypeUpdateRequest);

        // Assert
        helper
            .UserDataContext.AssetTypes.Single(at => at.ID == childAssetType.ID)
            .Parent.Should()
            .Be(string.Empty);
        helper
            .UserDataContext.AssetTypes.Single(at => at.ID == parentAssetType.ID)
            .Parent.Should()
            .Be(newParentType.Value);
    }
    #endregion

    #region DeleteAssetTypeAsync
    [Fact]
    public async Task DeleteAssetTypeAsync_WhenCalledWithValidData_ShouldDeleteAssetType()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);
        for (var i = 1; i < assetTypes.Count; i++)
        {
            assetTypes[i].Parent = assetTypes.First().Value;
        }

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        await assetTypeService.DeleteAssetTypeAsync(helper.demoUser.Id, assetTypes.Last().ID);

        // Assert
        helper.UserDataContext.AssetTypes.Should().NotContainEquivalentOf(assetTypes.Last());
    }

    [Fact]
    public async Task DeleteAssetTypeAsync_WhenCalledWithInvalidAssetTypeID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetTypes = assetTypeFaker.Generate(5);

        helper.UserDataContext.AssetTypes.AddRange(assetTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await assetTypeService.DeleteAssetTypeAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetTypeNotFoundError");
    }

    [Fact]
    public async Task DeleteAssetTypeAsync_WhenAssetTypeHasChildren_ShouldDeleteChildrenAndResetAssets()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var parentAssetType = assetTypeFaker.Generate();
        var childAssetType = assetTypeFaker.Generate();
        childAssetType.Parent = parentAssetType.Value;

        helper.UserDataContext.AssetTypes.AddRange([parentAssetType, childAssetType]);

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();
        asset.Type = childAssetType.Value;

        helper.UserDataContext.Assets.Add(asset);
        helper.UserDataContext.SaveChanges();

        // Act
        await assetTypeService.DeleteAssetTypeAsync(helper.demoUser.Id, parentAssetType.ID);

        // Assert
        helper.UserDataContext.AssetTypes.Should().NotContain(parentAssetType);
        helper.UserDataContext.AssetTypes.Should().NotContain(childAssetType);
        asset.Type.Should().Be(string.Empty);
    }

    [Fact]
    public async Task DeleteAssetTypeAsync_WhenAssetTypeInUseByAsset_ShouldResetAssetType()
    {
        // Arrange
        var helper = new TestHelper();

        var assetTypeService = new AssetTypeService(
            Mock.Of<ILogger<IAssetTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetTypeFaker = new AssetTypeFaker(helper.demoUser.Id);
        var assetType = assetTypeFaker.Generate();

        helper.UserDataContext.AssetTypes.Add(assetType);

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();
        asset.Type = assetType.Value;

        helper.UserDataContext.Assets.Add(asset);
        helper.UserDataContext.SaveChanges();

        // Act
        await assetTypeService.DeleteAssetTypeAsync(helper.demoUser.Id, assetType.ID);

        // Assert
        helper.UserDataContext.AssetTypes.Should().NotContain(assetType);
        asset.Type.Should().Be(string.Empty);
    }
    #endregion
}
