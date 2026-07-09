using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class AssetService(
    ILogger<IAssetService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAssetService
{
    /// <inheritdoc />
    public async Task CreateAssetAsync(Guid userGuid, IAssetCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var newAsset = new Asset { Name = request.Name, UserID = userData.Id };

        userDataContext.Assets.Add(newAsset);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAssetResponse>> ReadAssetsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        return userData.Assets.OrderBy(a => a.Index).Select(a => new AssetResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAssetAsync(Guid userGuid, IAssetUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, request.ID);

        if (request.Name.IsSpecified && !string.IsNullOrWhiteSpace(request.Name.Value))
        {
            asset.Name = request.Name.Value;
        }

        if (request.PurchaseDate.IsSpecified)
        {
            asset.PurchaseDate = request.PurchaseDate.Value;
        }

        if (request.PurchasePrice.IsSpecified)
        {
            asset.PurchasePrice = request.PurchasePrice.Value;
        }

        if (request.SellDate.IsSpecified)
        {
            asset.SellDate = request.SellDate.Value;
        }

        if (request.SellPrice.IsSpecified)
        {
            asset.SellPrice = request.SellPrice.Value;
        }

        if (request.Hide.IsSpecified)
        {
            asset.Hide = request.Hide.Value;
        }

        if (request.Type.IsSpecified)
        {
            asset.Type = request.Type.Value ?? string.Empty;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, assetGuid);

        asset.Type = string.Empty;
        asset.Deleted = nowProvider.UtcNow;
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, assetGuid);

        asset.Deleted = null;
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task OrderAssetsAsync(Guid userGuid, IEnumerable<IAssetIndexRequest> orderedAssets)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        foreach (var orderedAsset in orderedAssets)
        {
            var asset = GetAssetById(userData, orderedAsset.ID);
            asset.Index = orderedAsset.Index;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task PermanentlyDeleteAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, assetGuid);
        if (asset.Deleted == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AssetPermanentDeleteNotDeletedLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AssetPermanentDeleteNotDeletedError"]
            );
        }

        userDataContext.Values.RemoveRange(asset.Values);
        userDataContext.Assets.Remove(asset);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users.Include(u => u.Assets).ThenInclude(a => a.Values)
        );
    }

    private Asset GetAssetById(ApplicationUser userData, Guid assetGuid)
    {
        var asset = userData.Assets.SingleOrDefault(a => a.ID == assetGuid);
        if (asset == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AssetNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AssetNotFoundError"]);
        }
        return asset;
    }
}
