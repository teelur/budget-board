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

public class AssetTypeService(
    ILogger<IAssetTypeService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAssetTypeService
{
    /// <inheritdoc />
    public async Task CreateAssetTypeAsync(Guid userGuid, IAssetTypeCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var allAssetTypes = AssetTypeHelpers.GetAllAssetTypes(userData);

        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueAlreadyExists(request.Value, allAssetTypes);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent, allAssetTypes);
        ThrowIfParentNotFound(request.Parent, allAssetTypes);

        var newAssetType = new AssetType
        {
            Value = request.Value,
            Parent = request.Parent,
            UserID = userData.Id,
        };

        userDataContext.AssetTypes.Add(newAssetType);
        await userDataContext.SaveChangesAsync();

        void ThrowIfValueAlreadyExists(string value, IEnumerable<IAssetTypeResponse> allAssetTypes)
        {
            if (allAssetTypes.Any(a => a.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeCreateDuplicateNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeCreateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeCreateEmptyNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeCreateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(
            string value,
            string parentValue,
            IEnumerable<IAssetTypeResponse> allAssetTypes
        )
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeCreateSameNameAsParentLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeCreateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(
            string parentValue,
            IEnumerable<IAssetTypeResponse> allAssetTypes
        )
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !allAssetTypes.Any(a =>
                    a.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeCreateParentNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeCreateParentNotFoundError"]
                );
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAssetTypeResponse>> ReadAssetTypesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return AssetTypeHelpers.GetAllAssetTypes(userData);
    }

    /// <inheritdoc />
    public async Task UpdateAssetTypeAsync(Guid userGuid, IAssetTypeUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var assetType = userData.AssetTypes.FirstOrDefault(a => a.ID == request.ID);
        if (assetType == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AssetTypeUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AssetTypeUpdateNotFoundError"]
            );
        }

        var allAssetTypes = AssetTypeHelpers.GetAllAssetTypes(userData);

        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueAlreadyExists(request.Value, allAssetTypes);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent);
        ThrowIfParentNotFound(request.Parent, allAssetTypes);

        var oldValue = assetType.Value;

        assetType.Value = request.Value;
        assetType.Parent = request.Parent;

        UpdateAssetsUsingType(userData.Assets, oldValue, request.Value);

        UpdateChildrenParentValue(userData.AssetTypes, oldValue, request.Value);

        await userDataContext.SaveChangesAsync();

        void ThrowIfValueAlreadyExists(string value, IEnumerable<IAssetTypeResponse> allAssetTypes)
        {
            if (
                allAssetTypes.Any(a =>
                    a.ID != request.ID && a.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeUpdateDuplicateNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeUpdateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeUpdateEmptyNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeUpdateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(string value, string parentValue)
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeUpdateSameNameAsParentLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeUpdateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(
            string parentValue,
            IEnumerable<IAssetTypeResponse> allAssetTypes
        )
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !allAssetTypes.Any(a =>
                    a.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeUpdateParentNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeUpdateParentNotFoundError"]
                );
            }
        }

        static void UpdateAssetsUsingType(ICollection<Asset> assets, string oldType, string newType)
        {
            foreach (var asset in assets)
            {
                if (
                    (asset.Type ?? string.Empty).Equals(oldType, StringComparison.OrdinalIgnoreCase)
                )
                    asset.Type = newType;
            }
        }

        static void UpdateChildrenParentValue(
            ICollection<AssetType> assetTypes,
            string oldParentValue,
            string newParentValue
        )
        {
            foreach (
                var child in assetTypes.Where(a =>
                    a.Parent.Equals(oldParentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.Parent = newParentValue;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteAssetTypeAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var assetType = userData.AssetTypes.FirstOrDefault(a => a.ID == guid);
        if (assetType == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AssetTypeDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AssetTypeDeleteNotFoundError"]
            );
        }

        RemoveChildrenUsingType(assetType.Value);
        UpdateAssetsUsingType(userData.Assets, assetType.Value, null);

        userData.AssetTypes.Remove(assetType);
        await userDataContext.SaveChangesAsync();

        void RemoveChildrenUsingType(string parentValue)
        {
            var children = userData
                .AssetTypes.Where(a =>
                    a.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
            foreach (var child in children)
            {
                UpdateAssetsUsingType(userData.Assets, child.Value, null);
                userData.AssetTypes.Remove(child);
            }
        }

        static void UpdateAssetsUsingType(
            ICollection<Asset> assets,
            string oldType,
            string? newType
        )
        {
            foreach (var asset in assets)
            {
                if (
                    (asset.Type ?? string.Empty).Equals(oldType, StringComparison.OrdinalIgnoreCase)
                )
                    asset.Type = newType;
            }
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users =>
                users.Include(u => u.AssetTypes).Include(u => u.Assets).Include(u => u.UserSettings)
        );
    }
}
