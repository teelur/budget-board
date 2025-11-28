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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var newAsset = _assetCreateRequestFaker.Generate();

        // Act
        await assetService.CreateAssetAsync(helper.demoUser.Id, newAsset);

        // Assert
        helper.demoUser.Assets.Should().ContainSingle(a => a.Name == newAsset.Name);
        helper.demoUser.Assets.Single().Name.Should().Be(newAsset.Name);
    }

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
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
    public async Task ReadAssetsAsync_WhenSingleAssetDoesNotExist_ShouldThrowError()
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
        var act = () => assetService.ReadAssetsAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AssetNotFoundError");
    }

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
            PurchaseDate = existingAsset.PurchaseDate,
            PurchasePrice = existingAsset.PurchasePrice,
            SellDate = existingAsset.SellDate,
            SellPrice = existingAsset.SellPrice,
            Hide = existingAsset.Hide,
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updatedAsset = new AssetUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Asset Name",
            PurchaseDate = null,
            PurchasePrice = null,
            SellDate = null,
            SellPrice = null,
            Hide = false,
        };

        // Act
        Func<Task> act = async () =>
            await assetService.UpdateAssetAsync(helper.demoUser.Id, updatedAsset);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("AssetEditNotFoundError");
    }

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

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await assetService.DeleteAssetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("AssetDeleteNotFoundError");
    }

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

        var assetFaker = new AssetFaker();
        var existingAsset = assetFaker.Generate();
        existingAsset.UserID = helper.demoUser.Id;
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await assetService.RestoreAssetAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("AssetRestoreNotFoundError");
    }

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

        var assetFaker = new AssetFaker();
        var assets = assetFaker.Generate(10);
        var rnd = new Random();
        assets = assets.OrderBy(a => rnd.Next()).ToList();
        foreach (var asset in assets)
        {
            asset.UserID = helper.demoUser.Id;
            asset.Index = assets.IndexOf(asset);
        }

        helper.UserDataContext.Assets.AddRange(assets);
        await helper.UserDataContext.SaveChangesAsync();

        var newOrder = new List<IAssetIndexRequest>();
        List<Asset> shuffledAssets = [.. assets.OrderBy(a => rnd.Next())];
        foreach (var asset in shuffledAssets)
        {
            newOrder.Add(new AssetIndexRequest { ID = asset.ID, Index = newOrder.Count });
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

    [Fact]
    public async Task OrderAssetsAsync_WhenAssetDoesNotExist_ShouldThrowException()
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

        var newOrder = new List<IAssetIndexRequest>
        {
            new AssetIndexRequest { ID = Guid.NewGuid(), Index = 0 },
        };

        // Act
        Func<Task> act = async () =>
            await assetService.OrderAssetsAsync(helper.demoUser.Id, newOrder);

        // Assert
        await act.Should().ThrowAsync<BudgetBoardServiceException>("AssetReorderNotFoundError");
    }
}
