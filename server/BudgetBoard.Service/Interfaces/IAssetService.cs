using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAssetService
{
    Task CreateAssetAsync(Guid userGuid, IAssetCreateRequest asset);
    Task<IEnumerable<IAssetResponse>> ReadAssetsAsync(Guid userGuid, Guid assetGuid = default);
    Task UpdateAssetAsync(Guid userGuid, IAssetUpdateRequest editedAsset);
    Task DeleteAssetAsync(Guid userGuid, Guid assetGuid);
    Task RestoreAssetAsync(Guid userGuid, Guid assetGuid);
    Task OrderAssetsAsync(Guid userGuid, IEnumerable<IAssetIndexRequest> orderedAssets);
}
