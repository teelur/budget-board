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
    public async Task CreateAssetAsync(Guid userGuid, IAssetCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var newAsset = new Asset { Name = request.Name, UserID = userData.Id };

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

        var assets = userData.Assets.ToList();

        if (assetGuid != default)
        {
            assets = [.. assets.Where(a => a.ID == assetGuid)];
            if (assets.Count == 0)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["AssetNotFoundLog"]);
                throw new BudgetBoardServiceException(_responseLocalizer["AssetNotFoundError"]);
            }
        }

        return assets.OrderBy(a => a.Index).Select(a => new AssetResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAssetAsync(Guid userGuid, IAssetUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.SingleOrDefault(a => a.ID == request.ID);
        if (asset == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AssetEditNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AssetEditNotFoundError"]);
        }

        asset.Name = request.Name;
        asset.PurchaseDate = request.PurchaseDate?.ToUniversalTime();
        asset.PurchasePrice = request.PurchasePrice;
        asset.SellDate = request.SellDate?.ToUniversalTime();
        asset.SellPrice = request.SellPrice;
        asset.Hide = request.Hide;

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAssetAsync(Guid userGuid, Guid assetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.SingleOrDefault(a => a.ID == assetGuid);
        if (asset == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AssetDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AssetDeleteNotFoundError"]);
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
            _logger.LogError("{LogMessage}", _logLocalizer["AssetRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AssetRestoreNotFoundError"]);
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
                _logger.LogError("{LogMessage}", _logLocalizer["AssetOrderNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["AssetOrderNotFoundError"]
                );
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
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Assets)
                .ThenInclude(a => a.Values)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
