using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing user assets.
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Creates a new asset for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The asset creation details.</param>
    Task CreateAssetAsync(Guid userGuid, IAssetCreateRequest request);

    /// <summary>
    /// Retrieves assets for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="assetGuid">Optional. The unique identifier of a specific asset to retrieve.</param>
    /// <returns>A collection of asset details.</returns>
    Task<IReadOnlyList<IAssetResponse>> ReadAssetsAsync(Guid userGuid, Guid assetGuid = default);

    /// <summary>
    /// Updates an existing asset for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The asset update details.</param>
    Task UpdateAssetAsync(Guid userGuid, IAssetUpdateRequest request);

    /// <summary>
    /// Deletes (soft deletes) an asset for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="assetGuid">The unique identifier of the asset to delete.</param>
    Task DeleteAssetAsync(Guid userGuid, Guid assetGuid);

    /// <summary>
    /// Restores a previously deleted asset for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="assetGuid">The unique identifier of the asset to restore.</param>
    Task RestoreAssetAsync(Guid userGuid, Guid assetGuid);

    /// <summary>
    /// Updates the order of assets for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="orderedAssets">A collection of asset index requests defining the new order.</param>
    Task OrderAssetsAsync(Guid userGuid, IEnumerable<IAssetIndexRequest> orderedAssets);
}
