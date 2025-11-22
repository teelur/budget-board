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
    private readonly ILogger<IAssetService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateAssetAsync(Guid userGuid, IAssetCreateRequest asset)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var newAsset = new Asset { Name = asset.Name, UserID = userData.Id };

        _userDataContext.Assets.Add(newAsset);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAssetResponse>> ReadAssetsAsync(
        Guid userGuid,
        Guid assetGuid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var assetsQuery = userData.Assets.ToList();

        if (assetGuid != default)
        {
            assetsQuery = [.. assetsQuery.Where(a => a.ID == assetGuid)];
            if (assetsQuery.Count == 0)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["AssetNotFoundLog"]);
                throw new BudgetBoardServiceException(_responseLocalizer["AssetNotFoundError"]);
            }
        }

        return assetsQuery.OrderBy(a => a.Index).Select(a => new AssetResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAssetAsync(Guid userGuid, IAssetUpdateRequest editedAsset)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.SingleOrDefault(a => a.ID == editedAsset.ID);

        if (asset == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AssetEditNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AssetEditNotFoundError"]);
        }

        if (userData.Assets.Any(a => a.Name == editedAsset.Name && a.ID != editedAsset.ID))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["DuplicateAssetNameLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["DuplicateAssetNameError"]);
        }

        _userDataContext.Entry(asset).CurrentValues.SetValues(editedAsset);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.SingleOrDefault(a => a.ID == assetGuid);
        if (asset == null)
        {
            _logger.LogError("Attempted to delete a non-existent asset.");
            throw new BudgetBoardServiceException("Asset not found.");
        }

        asset.Deleted = _nowProvider.UtcNow;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.SingleOrDefault(a => a.ID == assetGuid);
        if (asset == null)
        {
            _logger.LogError("Attempted to restore a non-existent asset.");
            throw new BudgetBoardServiceException("Asset not found.");
        }

        asset.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task OrderAssetsAsync(Guid userGuid, IEnumerable<IAssetIndexRequest> orderedAssets)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var orderedAsset in orderedAssets)
        {
            var asset = userData.Assets.SingleOrDefault(a => a.ID == orderedAsset.ID);
            if (asset == null)
            {
                _logger.LogError("Attempted to order a non-existent asset.");
                throw new BudgetBoardServiceException("Asset not found.");
            }
            asset.Index = orderedAsset.Index;
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            var users = await _userDataContext
                .ApplicationUsers.Include(u => u.Assets)
                .ThenInclude(a => a.Values)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
