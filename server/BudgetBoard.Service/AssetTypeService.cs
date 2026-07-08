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
        var userData = await GetCurrentUserAsync(userGuid);
        var allAssetTypes = GetAllAssetTypes(userData);

        ValidateAssetTypeData(request.Value, request.Parent, allAssetTypes);

        var newAssetType = new AssetType
        {
            Value = request.Value,
            Parent = request.Parent,
            UserID = userData.Id,
        };

        userDataContext.AssetTypes.Add(newAssetType);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAssetTypeResponse>> ReadAssetTypesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        return GetAllAssetTypes(userData);
    }

    /// <inheritdoc />
    public async Task UpdateAssetTypeAsync(Guid userGuid, IAssetTypeUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var assetType = GetAssetTypeById(userData, request.ID);
        var allAssetTypes = GetAllAssetTypes(userData);

        ValidateAssetTypeData(
            request.Value ?? assetType.Value,
            request.Parent ?? assetType.Parent,
            allAssetTypes.Where(a => a.ID != request.ID)
        );

        var oldValue = assetType.Value;
        var oldParent = assetType.Parent;

        if (
            request.Value != null
            && !request.Value.Equals(oldValue, StringComparison.OrdinalIgnoreCase)
        )
        {
            assetType.Value = request.Value;
            UpdateAssetsUsingType(userData.Assets, oldValue, request.Value);
            UpdateChildrenParentValue(userData.AssetTypes, oldValue, request.Value);
        }
        if (request.Parent != null)
        {
            assetType.Parent = request.Parent;
            if (
                string.IsNullOrEmpty(oldParent)
                && !request.Parent.Equals(oldParent, StringComparison.OrdinalIgnoreCase)
            )
            {
                UpdateOrphanedChildren(userData.AssetTypes, oldValue);
            }
        }

        await userDataContext.SaveChangesAsync();

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

        static void UpdateOrphanedChildren(ICollection<AssetType> assetTypes, string oldParentValue)
        {
            foreach (
                var child in assetTypes.Where(a =>
                    a.Parent.Equals(oldParentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.Parent = string.Empty;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteAssetTypeAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var assetType = GetAssetTypeById(userData, guid);

        RemoveChildrenUsingType(assetType.Value);
        UpdateAssetsUsingType(userData.Assets, assetType.Value, string.Empty);

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
                UpdateAssetsUsingType(userData.Assets, child.Value, string.Empty);
                userData.AssetTypes.Remove(child);
            }
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
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

    private static List<IAssetTypeResponse> GetAllAssetTypes(ApplicationUser userData)
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

    private AssetType GetAssetTypeById(ApplicationUser userData, Guid id)
    {
        var assetType = userData.AssetTypes.FirstOrDefault(a => a.ID == id);
        if (assetType == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AssetTypeNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AssetTypeNotFoundError"]);
        }

        return assetType;
    }

    private void ValidateAssetTypeData(
        string value,
        string parentValue,
        IEnumerable<IAssetTypeResponse> allAssetTypes
    )
    {
        ThrowIfValueIsNullOrEmpty(value);
        ThrowIfValueAlreadyExists(value, allAssetTypes);
        ThrowIfValueSameNameAsParent(value, parentValue, allAssetTypes);
        ThrowIfParentNotFound(parentValue, allAssetTypes);

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeEmptyNameLog"]);
                throw new BudgetBoardServiceException(responseLocalizer["AssetTypeEmptyNameError"]);
            }
        }

        void ThrowIfValueAlreadyExists(string value, IEnumerable<IAssetTypeResponse> allAssetTypes)
        {
            if (allAssetTypes.Any(a => a.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeDuplicateNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeDuplicateNameError"]
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
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeSameNameAsParentLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeSameNameAsParentError"]
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
                logger.LogError("{LogMessage}", logLocalizer["AssetTypeParentNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AssetTypeParentNotFoundError"]
                );
            }
        }
    }

    private static void UpdateAssetsUsingType(
        ICollection<Asset> assets,
        string oldType,
        string newType
    )
    {
        foreach (var asset in assets)
        {
            if (asset.Type.Equals(oldType, StringComparison.OrdinalIgnoreCase))
            {
                asset.Type = newType;
            }
        }
    }
}
