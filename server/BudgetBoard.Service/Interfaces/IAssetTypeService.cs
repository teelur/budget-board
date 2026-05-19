using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing asset types.
/// </summary>
public interface IAssetTypeService
{
    /// <summary>
    /// Creates a new asset type for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The asset type creation details.</param>
    Task CreateAssetTypeAsync(Guid userGuid, IAssetTypeCreateRequest request);

    /// <summary>
    /// Retrieves all asset types for the specified user. This includes both built-in (if configured) and custom asset types.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A collection of asset type details.</returns>
    Task<IReadOnlyList<IAssetTypeResponse>> ReadAssetTypesAsync(Guid userGuid);

    /// <summary>
    /// Updates an existing asset type.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The asset type update details.</param>
    Task UpdateAssetTypeAsync(Guid userGuid, IAssetTypeUpdateRequest request);

    /// <summary>
    /// Deletes an asset type.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the asset type to delete.</param>
    Task DeleteAssetTypeAsync(Guid userGuid, Guid guid);
}
