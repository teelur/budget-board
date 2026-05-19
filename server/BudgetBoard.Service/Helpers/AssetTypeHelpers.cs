using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

internal static class AssetTypeHelpers
{
    /// <summary>
    /// Combines built-in and custom asset types for the specified user.
    /// </summary>
    /// <param name="userData">The user whose asset types are to be retrieved.</param>
    /// <returns>A read-only list containing all applicable asset types.</returns>
    internal static IReadOnlyList<IAssetTypeResponse> GetAllAssetTypes(ApplicationUser userData)
    {
        var allAssetTypes = new List<IAssetTypeResponse>();
        allAssetTypes.AddRange(
            userData.AssetTypes.Select(at => new AssetTypeResponse(at)).ToList()
        );

        if (userData.UserSettings?.DisableBuiltInAssetTypes != true)
        {
            allAssetTypes.AddRange(
                AssetTypeConstants
                    .DefaultAssetTypes.Select(at => new AssetTypeResponse(at))
                    .ToList()
            );
        }

        return allAssetTypes;
    }
}
