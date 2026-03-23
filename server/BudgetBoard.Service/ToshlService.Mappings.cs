using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.Service;

public partial class ToshlService
{
    private const string ToshlCategoryType = "category";
    private const string ToshlTagType = "tag";

    private static readonly JsonSerializerOptions s_toshlMappingsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly IReadOnlyDictionary<string, string> s_toshlCategoryDefaults =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["food drinks"] = "Food & Dining",
            ["food and drinks"] = "Food & Dining",
            ["clothing footwear"] = "Shopping",
            ["clothing and footwear"] = "Shopping",
            ["home utilities"] = "Home",
            ["home and utilities"] = "Home",
            ["health personal care"] = "Health & Fitness",
            ["health and personal care"] = "Health & Fitness",
            ["transport"] = "Auto & Transport",
            ["leisure"] = "Entertainment",
            ["education"] = "Education",
            ["gifts"] = "Gifts & Donations",
            ["loans"] = "Loans",
            ["taxes"] = "Taxes",
            ["subscriptions"] = "Shopping",
            ["hobby"] = "Hobbies",
            ["salary"] = "Paycheck",
            ["reimbursements"] = "Reimbursements",
            ["transfer"] = "Transfer",
            ["other"] = "Other",
            ["groceries"] = "Groceries",
            ["restaurants"] = "Restaurants",
            ["soft drinks"] = "Groceries",
            ["alcohol"] = "Alcohol & Bars",
            ["beer"] = "Alcohol & Bars",
            ["coffee tea"] = "Coffee Shops",
            ["coffee and tea"] = "Coffee Shops",
            ["fast food"] = "Food Delivery",
            ["takeaway"] = "Food Delivery",
            ["clothes"] = "Clothing",
            ["shoes"] = "Clothing",
            ["rent"] = "Mortgage & Rent",
            ["furniture"] = "Furnishings",
            ["home improvement"] = "Home Improvement",
            ["devices"] = "Electronics & Software",
            ["water"] = "Utilities",
            ["building upkeep"] = "Home Services",
            ["heating"] = "Utilities",
            ["electricity"] = "Utilities",
            ["mobile phone"] = "Mobile Phone",
            ["internet"] = "Internet",
            ["medicine"] = "Pharmacy",
            ["medical services"] = "Doctor",
            ["cosmetics"] = "Personal Care",
            ["dentist"] = "Dentist",
            ["gym"] = "Gym",
            ["car"] = "Auto & Transport",
            ["train"] = "Public Transportation",
            ["bus"] = "Public Transportation",
            ["metro"] = "Public Transportation",
            ["airplane"] = "Air Travel",
            ["taxi"] = "Rental Car & Taxi",
            ["travel"] = "Travel",
            ["events"] = "Activities",
            ["books"] = "Books",
            ["movies tv"] = "Movies",
            ["movies and tv"] = "Movies",
            ["apps"] = "Electronics & Software",
            ["tuition"] = "Tuition",
            ["birthday"] = "Gift",
            ["gift"] = "Gift",
            ["loan repayment"] = "Loan Payments",
            ["insurance"] = "Loan Insurance",
            ["property tax"] = "Property Tax",
            ["income tax"] = "Taxes",
            ["services"] = "Electronics & Software",
            ["spotify"] = "Electronics & Software",
            ["youtube"] = "Electronics & Software",
            ["chatgpt"] = "Electronics & Software",
            ["vpn"] = "Electronics & Software",
        };

    public async Task<IToshlCategoryMappingsResponse> ReadCategoryMappingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var remoteCategories = await GetRemoteItemsAsync<ToshlMetadataItem>(
            ToshlCategoriesEndpoint,
            userData
        );
        var remoteTags = await GetRemoteItemsAsync<ToshlTagItem>(ToshlTagsEndpoint, userData);

        var items = BuildResolvedMappingItems(userData, remoteCategories, remoteTags);
        return new ToshlCategoryMappingsResponse { Items = items };
    }

    public async Task UpdateCategoryMappingsAsync(
        Guid userGuid,
        IToshlCategoryMappingsUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            throw new BudgetBoardServiceException("User settings are required.");
        }

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var cleanedMappings = request
            .Items.Where(i =>
                !string.IsNullOrWhiteSpace(i.ToshlId)
                && !string.IsNullOrWhiteSpace(i.ToshlType)
                && !string.IsNullOrWhiteSpace(i.BudgetBoardCategory)
            )
            .Select(i => new ToshlCategoryMappingUpdateItem
            {
                ToshlId = i.ToshlId.Trim(),
                ToshlName = i.ToshlName.Trim(),
                ToshlType = i.ToshlType.Trim().ToLowerInvariant(),
                BudgetBoardCategory = ValidateMappedCategory(
                    i.BudgetBoardCategory.Trim(),
                    allCategories
                ),
            })
            .Where(i => i.ToshlType == ToshlCategoryType || i.ToshlType == ToshlTagType)
            .GroupBy(i => BuildToshlMappingLookupKey(i.ToshlType, i.ToshlId))
            .Select(g => g.First())
            .OrderBy(i => i.ToshlType)
            .ThenBy(i => i.ToshlId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        userSettings.ToshlCategoryMappingsJson = JsonSerializer.Serialize(
            cleanedMappings,
            s_toshlMappingsJsonOptions
        );

        await ApplyMappingsToExistingTransactionsAsync(userData);
        await userDataContext.SaveChangesAsync();
    }

    private async Task ApplyMappingsToExistingTransactionsAsync(ApplicationUser userData)
    {
        var remoteCategories = await GetRemoteItemsAsync<ToshlMetadataItem>(
            ToshlCategoriesEndpoint,
            userData
        );
        var remoteTags = await GetRemoteItemsAsync<ToshlTagItem>(ToshlTagsEndpoint, userData);
        var entriesEndpoint = BuildToshlEntriesEndpoint(
            ToshlEntriesEndpoint,
            0,
            DateTime.MinValue,
            fullSync: true
        );
        var remoteEntries = await GetRemoteItemsAsync<ToshlEntryItem>(entriesEndpoint, userData);

        var categoryNamesById = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToDictionary(c => c.Id!, c => c.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var tagNamesById = remoteTags
            .Where(t => !string.IsNullOrWhiteSpace(t.Id))
            .ToDictionary(t => t.Id!, t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var resolvedMappings = BuildResolvedMappings(userData, remoteCategories, remoteTags);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        var localTransactions = await userDataContext
            .Transactions.Include(t => t.Account)
            .ThenInclude(a => a!.Institution)
            .Where(t =>
                t.Account != null
                && t.Account.UserID == userData.Id
                && t.Account.Institution != null
                && EF.Functions.ILike(t.Account.Institution.Name, ToshlImportInstitutionName)
                && !string.IsNullOrWhiteSpace(t.SyncID)
            )
            .ToDictionaryAsync(t => t.SyncID!, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in remoteEntries.Where(e => e.Deleted != true && !string.IsNullOrWhiteSpace(e.Id)))
        {
            if (!localTransactions.TryGetValue(entry.Id!, out var transaction))
            {
                continue;
            }

            var merchantName = ResolveToshlEntryMerchantName(entry, tagNamesById);
            var mappedCategory = ResolveToshlEntryCategory(
                entry,
                categoryNamesById,
                tagNamesById,
                resolvedMappings
            );
            ApplyResolvedToshlTransactionMetadata(
                transaction,
                merchantName,
                mappedCategory,
                allCategories
            );
        }
    }

    private IReadOnlyList<ToshlCategoryMappingItem> BuildResolvedMappingItems(
        ApplicationUser userData,
        IReadOnlyList<ToshlMetadataItem> remoteCategories,
        IReadOnlyList<ToshlTagItem> remoteTags
    )
    {
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var savedMappings = GetSavedMappings(userData.UserSettings);
        var remoteCategoryById = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToDictionary(c => c.Id!, c => c, StringComparer.OrdinalIgnoreCase);
        var items = new List<ToshlCategoryMappingItem>();

        foreach (var remoteCategory in remoteCategories.Where(c =>
                     !string.IsNullOrWhiteSpace(c.Id)
                     && !string.IsNullOrWhiteSpace(c.Name)
                     && c.Deleted != true
                     && !string.Equals(c.Type, "system", StringComparison.OrdinalIgnoreCase)
                 ))
        {
            var parentName = string.Empty;
            if (
                !string.IsNullOrWhiteSpace(remoteCategory.Category)
                && remoteCategoryById.TryGetValue(remoteCategory.Category!, out var parentCategory)
            )
            {
                parentName = parentCategory.Name ?? string.Empty;
            }

            var suggested = GetSuggestedBudgetBoardCategory(
                remoteCategory.Name!,
                ToshlCategoryType,
                parentName,
                allCategories
            );
            var lookupKey = BuildToshlMappingLookupKey(ToshlCategoryType, remoteCategory.Id!);
            var legacyLookupKey = BuildToshlLegacyMappingLookupKey(
                ToshlCategoryType,
                remoteCategory.Name!
            );

            items.Add(
                new ToshlCategoryMappingItem
                {
                    ToshlId = remoteCategory.Id!,
                    ToshlName = remoteCategory.Name!,
                    ToshlType = ToshlCategoryType,
                    ToshlParentName = parentName,
                    SuggestedBudgetBoardCategory = suggested,
                    BudgetBoardCategory = savedMappings.TryGetValue(lookupKey, out var mappedValue)
                        ? mappedValue
                        : savedMappings.TryGetValue(legacyLookupKey, out var legacyMappedValue)
                            ? legacyMappedValue
                        : suggested,
                }
            );
        }

        foreach (var remoteTag in remoteTags.Where(t =>
                     !string.IsNullOrWhiteSpace(t.Id)
                     && !string.IsNullOrWhiteSpace(t.Name)
                     && t.Deleted != true
                 ))
        {
            var parentName = string.Empty;
            if (
                !string.IsNullOrWhiteSpace(remoteTag.Category)
                && remoteCategoryById.TryGetValue(remoteTag.Category!, out var parentCategory)
            )
            {
                parentName = parentCategory.Name ?? string.Empty;
            }

            var suggested = GetSuggestedBudgetBoardCategory(
                remoteTag.Name!,
                ToshlTagType,
                parentName,
                allCategories
            );
            var lookupKey = BuildToshlMappingLookupKey(ToshlTagType, remoteTag.Id!);
            var legacyLookupKey = BuildToshlLegacyMappingLookupKey(ToshlTagType, remoteTag.Name!);

            items.Add(
                new ToshlCategoryMappingItem
                {
                    ToshlId = remoteTag.Id!,
                    ToshlName = remoteTag.Name!,
                    ToshlType = ToshlTagType,
                    ToshlParentName = parentName,
                    SuggestedBudgetBoardCategory = suggested,
                    BudgetBoardCategory = savedMappings.TryGetValue(lookupKey, out var mappedValue)
                        ? mappedValue
                        : savedMappings.TryGetValue(legacyLookupKey, out var legacyMappedValue)
                            ? legacyMappedValue
                        : suggested,
                }
            );
        }

        return items
            .OrderBy(i => i.ToshlType == ToshlCategoryType ? 0 : 1)
            .ThenBy(i => i.ToshlParentName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.ToshlName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private Dictionary<string, string> BuildResolvedMappings(
        ApplicationUser userData,
        IReadOnlyList<ToshlMetadataItem> remoteCategories,
        IReadOnlyList<ToshlTagItem> remoteTags
    )
    {
        return BuildResolvedMappingItems(userData, remoteCategories, remoteTags)
            .Where(i => !string.IsNullOrWhiteSpace(i.BudgetBoardCategory))
            .ToDictionary(
                i => BuildToshlMappingLookupKey(i.ToshlType, i.ToshlId),
                i => i.BudgetBoardCategory,
                StringComparer.OrdinalIgnoreCase
            );
    }

    private static string ValidateMappedCategory(string mappedCategory, IEnumerable<ICategory> allCategories)
    {
        var matchedCategory = allCategories.FirstOrDefault(c =>
            c.Value.Equals(mappedCategory, StringComparison.OrdinalIgnoreCase)
        );
        if (matchedCategory == null)
        {
            throw new BudgetBoardServiceException(
                $"Unknown Budget Board category '{mappedCategory}'."
            );
        }

        return matchedCategory.Value;
    }

    private static Dictionary<string, string> GetSavedMappings(UserSettings? userSettings)
    {
        if (userSettings == null || string.IsNullOrWhiteSpace(userSettings.ToshlCategoryMappingsJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var items =
                JsonSerializer.Deserialize<List<ToshlCategoryMappingUpdateItem>>(
                    userSettings.ToshlCategoryMappingsJson,
                    s_toshlMappingsJsonOptions
                ) ?? new List<ToshlCategoryMappingUpdateItem>();

            return items
                .Where(i =>
                    !string.IsNullOrWhiteSpace(i.ToshlType)
                    && (
                        !string.IsNullOrWhiteSpace(i.ToshlId)
                        || !string.IsNullOrWhiteSpace(i.ToshlName)
                    )
                    && !string.IsNullOrWhiteSpace(i.BudgetBoardCategory)
                )
                .GroupBy(i =>
                    !string.IsNullOrWhiteSpace(i.ToshlId)
                        ? BuildToshlMappingLookupKey(i.ToshlType, i.ToshlId)
                        : BuildToshlLegacyMappingLookupKey(i.ToshlType, i.ToshlName)
                )
                .ToDictionary(
                    g => g.Key,
                    g => g.First().BudgetBoardCategory,
                    StringComparer.OrdinalIgnoreCase
                );
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string GetSuggestedBudgetBoardCategory(
        string toshlName,
        string toshlType,
        string toshlParentName,
        IEnumerable<ICategory> allCategories
    )
    {
        var exactCategory = allCategories.FirstOrDefault(c =>
            c.Value.Equals(toshlName, StringComparison.OrdinalIgnoreCase)
        );
        if (exactCategory != null)
        {
            return exactCategory.Value;
        }

        var normalizedName = NormalizeToshlMappingName(toshlName);
        if (s_toshlCategoryDefaults.TryGetValue(normalizedName, out var mappedCategory))
        {
            return mappedCategory;
        }

        if (!string.IsNullOrWhiteSpace(toshlParentName))
        {
            var normalizedParentName = NormalizeToshlMappingName(toshlParentName);
            if (
                toshlType == ToshlTagType
                && s_toshlCategoryDefaults.TryGetValue(normalizedParentName, out var parentMapped)
            )
            {
                return parentMapped;
            }
        }

        return string.Empty;
    }

    private static string BuildToshlMappingLookupKey(string toshlType, string toshlId)
    {
        return $"{toshlType.Trim().ToLowerInvariant()}::id::{toshlId.Trim().ToLowerInvariant()}";
    }

    private static string BuildToshlLegacyMappingLookupKey(string toshlType, string toshlName)
    {
        return
            $"{toshlType.Trim().ToLowerInvariant()}::name::{toshlName.Trim().ToLowerInvariant()}";
    }

    private static string NormalizeToshlMappingName(string value)
    {
        return new string(
                value
                    .Trim()
                    .ToLowerInvariant()
                    .Select(c => char.IsLetterOrDigit(c) ? c : ' ')
                    .ToArray()
            )
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(string.Empty, (current, next) =>
                string.IsNullOrEmpty(current) ? next : $"{current} {next}");
    }
}
