using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AssetServiceTests
{
    #region CreateAssetAsync
    [Fact]
    public async Task CreateAssetAsync_WhenValidData_ShouldCreateAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var newAsset = new AssetCreateRequest { Name = "New Asset" };

        // Act
        await assetService.CreateAssetAsync(helper.demoUser.Id, newAsset);

        // Assert
        helper.demoUser.Assets.Should().ContainSingle(a => a.Name == newAsset.Name);
        helper.demoUser.Assets.Single().Name.Should().Be(newAsset.Name);
    }
    #endregion

    #region ReadAssetsAsync
    [Fact]
    public async Task ReadAssetsAsync_WhenAssetsExist_ShouldReturnAssets()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset1 = assetFaker.Generate();
        asset1.Name = "Asset 1";
        asset1.Index = 0;

        var asset2 = assetFaker.Generate();
        asset2.Name = "Asset 2";
        asset2.Index = 1;

        helper.UserDataContext.Assets.AddRange(asset1, asset2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var assets = await assetService.ReadAssetsAsync(helper.demoUser.Id);

        // Assert
        assets.Should().HaveCount(2);
        assets.ElementAt(0).Should().BeEquivalentTo(new AssetResponse(asset1));
        assets.ElementAt(1).Should().BeEquivalentTo(new AssetResponse(asset2));
    }

    [Fact]
    public async Task ReadAssetsAsync_WhenAssetHasValues_ShouldReturnMostRecentValueAndDate()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();
        asset.Name = "Asset with Values";

        var value1 = new Database.Models.Value
        {
            Amount = 1000m,
            Date = new DateOnly(2020, 1, 1),
            AssetID = asset.ID,
        };

        var value2 = new Database.Models.Value
        {
            Amount = 1500m,
            Date = new DateOnly(2021, 1, 1),
            AssetID = asset.ID,
        };

        helper.UserDataContext.Assets.Add(asset);
        helper.UserDataContext.Values.AddRange(value1, value2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var assets = await assetService.ReadAssetsAsync(helper.demoUser.Id);

        // Assert
        assets.Should().HaveCount(1);
        var returnedAsset = assets.Single();
        returnedAsset.CurrentValue.Should().Be(value2.Amount);
        returnedAsset.ValueDate.Should().Be(value2.Date);
    }
    #endregion

    #region UpdateAssetAsync
    [Fact]
    public async Task UpdateAssetAsync_WhenValidData_ShouldUpdateAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var existingAsset = assetFaker.Generate();

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedAsset = new AssetUpdateRequest
        {
            ID = existingAsset.ID,
            Name = "Old Asset Name",
            PurchaseDate = new DateOnly(2020, 1, 1),
            PurchasePrice = 1000m,
            SellDate = new DateOnly(2021, 1, 1),
            SellPrice = 1500m,
            Hide = true,
            Type = "Real Estate",
        };

        // Act
        await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Name.Should().Be(updatedAsset.Name.Value);
        assetInDb.PurchaseDate.Should().Be(updatedAsset.PurchaseDate.Value);
        assetInDb.PurchasePrice.Should().Be(updatedAsset.PurchasePrice.Value);
        assetInDb.SellDate.Should().Be(updatedAsset.SellDate.Value);
        assetInDb.SellPrice.Should().Be(updatedAsset.SellPrice.Value);
        assetInDb.Hide.Should().Be(updatedAsset.Hide.Value);
        assetInDb.Type.Should().Be(updatedAsset.Type.Value);
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenAssetDoesNotExist_ShouldThrowAssetNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updatedAsset = new AssetUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Asset Name",
        };

        // Act
        Func<Task> act = async () =>
            await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("AssetNotFoundError");
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenPropertyIsOmitted_ShouldNotUpdateAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var existingAsset = assetFaker.Generate();
        existingAsset.Name = "Old Name";
        existingAsset.PurchasePrice = 1000m;
        existingAsset.PurchaseDate = new DateOnly(2020, 1, 1);
        existingAsset.SellDate = new DateOnly(2021, 1, 1);
        existingAsset.SellPrice = 1500m;
        existingAsset.Hide = true;
        existingAsset.Type = "Real Estate";

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedAsset = new AssetUpdateRequest { ID = existingAsset.ID };

        // Act
        await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Name.Should().Be("Old Name");
        assetInDb.PurchasePrice.Should().Be(1000m);
        assetInDb.PurchaseDate.Should().Be(new DateOnly(2020, 1, 1));
        assetInDb.SellDate.Should().Be(new DateOnly(2021, 1, 1));
        assetInDb.SellPrice.Should().Be(1500m);
        assetInDb.Hide.Should().Be(true);
        assetInDb.Type.Should().Be("Real Estate");
    }

    [Fact]
    public async Task UpdateAssetAsync_WhenNullablePropertyIsExplicitNull_ShouldClearValue()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var existingAsset = assetFaker.Generate();
        existingAsset.PurchasePrice = 2500m;

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedAsset = new AssetUpdateRequest
        {
            ID = existingAsset.ID,
            PurchasePrice = null,
            PurchaseDate = null,
            SellDate = null,
            SellPrice = null,
        };

        // Act
        await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.PurchasePrice.Should().BeNull();
        assetInDb.PurchaseDate.Should().BeNull();
        assetInDb.SellDate.Should().BeNull();
        assetInDb.SellPrice.Should().BeNull();
    }
    #endregion

    #region DeleteAssetAsync
    [Fact]
    public async Task DeleteAssetAsync_WhenAssetExists_ShouldDeleteAsset()
    {
        // Arrange
        var nowProviderMock = new Mock<INowProvider>();
        var fixedNow = new DateTime(2024, 1, 1);
        nowProviderMock.Setup(np => np.UtcNow).Returns(fixedNow);

        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var existingAsset = assetFaker.Generate();

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await assetService.DeleteAssetAsync(helper.demoUser.Id, existingAsset.ID);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Deleted.Should().Be(fixedNow);
        assetInDb.Type.Should().Be(string.Empty);
    }
    #endregion

    #region RestoreAssetAsync
    [Fact]
    public async Task RestoreAssetAsync_WhenAssetExists_ShouldRestoreAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var existingAsset = assetFaker.Generate();
        existingAsset.Deleted = DateTime.UtcNow.AddDays(-1);

        helper.UserDataContext.Assets.Add(existingAsset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await assetService.RestoreAssetAsync(helper.demoUser.Id, existingAsset.ID);

        // Assert
        var assetInDb = helper.UserDataContext.Assets.Single(a => a.ID == existingAsset.ID);
        assetInDb.Deleted.Should().BeNull();
    }
    #endregion

    #region OrderAssetsAsync
    [Fact]
    public async Task OrderAssetsAsync_WhenValidOrder_ShouldUpdateIndices()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var assets = assetFaker.Generate(10);
        var rnd = new Random();
        assets = [.. assets.OrderBy(a => rnd.Next())];
        assets.ForEach(asset => asset.Index = assets.IndexOf(asset));

        helper.UserDataContext.Assets.AddRange(assets);
        await helper.UserDataContext.SaveChangesAsync();

        var newOrder = new List<IAssetIndexRequest>();
        List<Asset> shuffledAssets = [.. assets.OrderBy(a => rnd.Next())];
        foreach (var asset in shuffledAssets)
        {
            newOrder.Add(
                new AssetIndexRequest { ID = asset.ID, Index = shuffledAssets.IndexOf(asset) }
            );
        }

        // Act
        await assetService.OrderAssetsAsync(helper.demoUser.Id, newOrder);

        // Assert
        helper
            .demoUser.Assets.OrderBy(a => a.Index)
            .Select(a => a.ID)
            .Should()
            .BeEquivalentTo(newOrder.OrderBy(o => o.Index).Select(o => o.ID));
    }
    #endregion

    #region PermanentDeleteAssetAsync
    [Fact]
    public async Task PermanentlyDeleteAssetAsync_DeletedAsset_ShouldRemoveAssetAndValues()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();
        asset.Deleted = new Faker().Date.Past().ToUniversalTime();

        var value1 = new Database.Models.Value
        {
            Amount = 100m,
            Date = new DateOnly(2024, 1, 1),
            AssetID = asset.ID,
        };

        var value2 = new Database.Models.Value
        {
            Amount = 200m,
            Date = new DateOnly(2024, 2, 1),
            AssetID = asset.ID,
        };

        helper.UserDataContext.Assets.Add(asset);
        helper.UserDataContext.Values.AddRange(value1, value2);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await assetService.PermanentlyDeleteAssetAsync(helper.demoUser.Id, asset.ID);

        // Assert
        helper.UserDataContext.Assets.Should().NotContain(a => a.ID == asset.ID);
        helper.UserDataContext.Values.Should().NotContain(v => v.AssetID == asset.ID);
    }

    [Fact]
    public async Task PermanentlyDeleteAssetAsync_WhenAssetNotDeleted_ShouldThrowAssetPermanentDeleteNotDeletedError()
    {
        // Arrange
        var helper = new TestHelper();
        var assetService = new AssetService(
            Mock.Of<ILogger<IAssetService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var assetFaker = new AssetFaker(helper.demoUser.Id);
        var asset = assetFaker.Generate();

        helper.UserDataContext.Assets.Add(asset);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var act = () => assetService.PermanentlyDeleteAssetAsync(helper.demoUser.Id, asset.ID);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetPermanentDeleteNotDeletedError");
    }
    #endregion
}
