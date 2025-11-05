using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AssetServiceTests
{
    private readonly Faker<AssetCreateRequest> _assetCreateRequestFaker =
        new Faker<AssetCreateRequest>().RuleFor(a => a.Name, f => f.Finance.AccountName());

    [Fact]
    public async Task CreateAssetAsync_WhenValidData_ShouldCreateAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var newAsset = _assetCreateRequestFaker.Generate();

        // Act
        await assetService.CreateAssetAsync(helper.demoUser.Id, newAsset);

        // Assert
        helper.demoUser.Assets.Should().ContainSingle(a => a.Name == newAsset.Name);
        helper.demoUser.Assets.Single().Name.Should().Be(newAsset.Name);
    }

    [Fact]
    public async Task CreateAssetAsync_WhenDuplicateName_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;
        existingAsset.Name = "Duplicate Asset";

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        var newAsset = _assetCreateRequestFaker.Generate();
        newAsset.Name = "Duplicate Asset";

        // Act
        Func<Task> act = async () =>
            await assetService.CreateAssetAsync(helper.demoUser.Id, newAsset);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("Asset already exists.");
    }

    [Fact]
    public async Task ReadAssetsAsync_WhenAssetsExist_ShouldReturnAssets()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var asset1 = assetFaker.Generate();
        asset1.UserID = helper.demoUser.Id;
        asset1.Name = "Asset 1";

        var asset2 = assetFaker.Generate();
        asset2.UserID = helper.demoUser.Id;
        asset2.Name = "Asset 2";

        helper.UserDataContext.Assets.AddRange(asset1, asset2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var assets = await assetService.ReadAssetsAsync(helper.demoUser.Id);

        // Assert
        assets.Should().HaveCount(2);
        assets.Should().ContainSingle(a => a.Name == "Asset 1");
        assets.Should().ContainSingle(a => a.Name == "Asset 2");
    }

    [Fact]
    public async Task ReadAssetsAsync_WhenReadingSingleAsset_ShouldReturnAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var asset1 = assetFaker.Generate();
        asset1.UserID = helper.demoUser.Id;
        asset1.Name = "Asset 1";

        var asset2 = assetFaker.Generate();
        asset2.UserID = helper.demoUser.Id;
        asset2.Name = "Asset 2";

        helper.UserDataContext.Assets.AddRange(asset1, asset2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var assets = await assetService.ReadAssetsAsync(helper.demoUser.Id, asset1.ID);

        // Assert
        assets.Should().HaveCount(1);
        assets.Should().ContainSingle(a => a.Name == "Asset 1");
    }

    [Fact]
    public async Task ReadAssetsAsync_WhenSingleAssetDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var asset1 = assetFaker.Generate();
        asset1.UserID = helper.demoUser.Id;
        asset1.Name = "Asset 1";

        var asset2 = assetFaker.Generate();
        asset2.UserID = helper.demoUser.Id;
        asset2.Name = "Asset 2";

        helper.UserDataContext.Assets.AddRange(asset1, asset2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var assets = await assetService.ReadAssetsAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        assets.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenValidData_ShouldUpdateAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;
        existingAsset.Name = "Old Asset Name";

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedAsset = new AssetUpdateRequest
        {
            ID = existingAsset.ID,
            Name = "Updated Asset Name",
            PurchasedDate = existingAsset.PurchasedDate,
            PurchasePrice = existingAsset.PurchasePrice,
            SoldDate = existingAsset.SoldDate,
            SoldPrice = existingAsset.SoldPrice,
            HideProperty = existingAsset.HideProperty,
        };

        // Act
        await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Name.Should().Be("Updated Asset Name");
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenAssetDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var updatedAsset = new AssetUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Asset Name",
            PurchasedDate = null,
            PurchasePrice = null,
            SoldDate = null,
            SoldPrice = null,
            HideProperty = false,
        };

        // Act
        Func<Task> act = async () =>
            await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("Asset not found.");
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenDuplicateName_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var existingAsset1 = assetFaker.Generate();
        existingAsset1.UserID = helper.demoUser.Id;
        existingAsset1.Name = "Asset 1";

        var existingAsset2 = assetFaker.Generate();
        existingAsset2.UserID = helper.demoUser.Id;
        existingAsset2.Name = "Asset 2";

        helper.UserDataContext.Assets.AddRange(existingAsset1, existingAsset2);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedAsset = new AssetUpdateRequest
        {
            ID = existingAsset2.ID,
            Name = "Asset 1", // Duplicate name
            PurchasedDate = existingAsset2.PurchasedDate,
            PurchasePrice = existingAsset2.PurchasePrice,
            SoldDate = existingAsset2.SoldDate,
            SoldPrice = existingAsset2.SoldPrice,
            HideProperty = existingAsset2.HideProperty,
        };

        // Act
        Func<Task> act = async () =>
            await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>("An asset with this name already exists.");
    }

    [Fact]
    public async Task DeleteAssetAsync_WhenAssetExists_ShouldDeleteAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        var fixedNow = new DateTime(2024, 1, 1);
        nowProviderMock.Setup(np => np.Now).Returns(fixedNow);

        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            nowProviderMock.Object
        );

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;
        existingAsset.Name = "Asset to Delete";

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await assetService.DeleteAssetAsync(helper.demoUser.Id, existingAsset.ID);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Deleted.Should().Be(fixedNow);
    }

    [Fact]
    public async Task DeleteAssetAsync_WhenAssetDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await assetService.DeleteAssetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("Asset not found.");
    }

    [Fact]
    public async Task RestoreAssetAsync_WhenAssetExists_ShouldRestoreAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;
        existingAsset.Name = "Asset to Restore";
        existingAsset.Deleted = DateTime.UtcNow.AddDays(-1);

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await assetService.RestoreAssetAsync(helper.demoUser.Id, existingAsset.ID);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Deleted.Should().BeNull();
    }

    [Fact]
    public async Task RestoreAssetAsync_WhenAssetDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        // Act
        Func<Task> act = async () =>
            await assetService.RestoreAssetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("Asset not found.");
    }

    [Fact]
    public async Task OrderAssetsAsync_WhenValidOrder_ShouldUpdateIndices()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var assetFaker = new AssetFaker();
        var asset1 = assetFaker.Generate();
        asset1.UserID = helper.demoUser.Id;
        asset1.Index = 0;

        var asset2 = assetFaker.Generate();
        asset2.UserID = helper.demoUser.Id;
        asset2.Index = 1;

        var asset3 = assetFaker.Generate();
        asset3.UserID = helper.demoUser.Id;
        asset3.Index = 2;

        helper.UserDataContext.Assets.AddRange(asset1, asset2, asset3);
        await helper.UserDataContext.SaveChangesAsync();

        var newOrder = new List<IAssetIndexRequest>
        {
            new AssetIndexRequest { ID = asset3.ID, Index = 0 },
            new AssetIndexRequest { ID = asset1.ID, Index = 1 },
            new AssetIndexRequest { ID = asset2.ID, Index = 2 },
        };

        // Act
        await assetService.OrderAssetsAsync(helper.demoUser.Id, newOrder);

        // Assert
        var assetsInDb = helper
            .UserDataContext.Assets.Where(a => a.UserID == helper.demoUser.Id)
            .ToList();

        assetsInDb.Single(a => a.ID == asset3.ID).Index.Should().Be(0);
        assetsInDb.Single(a => a.ID == asset1.ID).Index.Should().Be(1);
        assetsInDb.Single(a => a.ID == asset2.ID).Index.Should().Be(2);
    }

    [Fact]
    public async Task OrderAssetsAsync_WhenAssetDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var newOrder = new List<IAssetIndexRequest>
        {
            new AssetIndexRequest { ID = Guid.NewGuid(), Index = 0 },
        };

        // Act
        Func<Task> act = async () =>
            await assetService.OrderAssetsAsync(helper.demoUser.Id, newOrder);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("Asset not found.");
    }
}
