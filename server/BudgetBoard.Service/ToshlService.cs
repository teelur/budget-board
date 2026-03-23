using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
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

public partial class ToshlService(
    IHttpClientFactory clientFactory,
    ILogger<IToshlService> logger,
    UserDataContext userDataContext,
    ITransactionService transactionService,
    IBalanceService balanceService,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IToshlService
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> s_userSyncLocks = new();
    private const string ToshlBaseUrl = "https://api.toshl.com";
    private const string ToshlAccountsEndpoint = "/accounts";
    private const string ToshlBudgetsEndpoint = "/budgets";
    private const string ToshlCategoriesEndpoint = "/categories";
    private const string ToshlEntriesEndpoint = "/entries";
    private const string ToshlTagsEndpoint = "/tags";
    private const string ToshlImportInstitutionName = "Toshl";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task ConfigureAccessTokenAsync(Guid userGuid, string accessToken)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogError("Toshl access token is missing or empty.");
            throw new BudgetBoardServiceException("Toshl access token is required.");
        }

        userData.ToshlAccessToken = accessToken.Trim();
        userData.ToshlLastSync = DateTime.MinValue;
        if (userData.UserSettings != null)
        {
            userData.UserSettings.ToshlFullSyncStatus = ToshlFullSyncStatuses.Idle;
            userData.UserSettings.ToshlFullSyncQueuedAt = null;
            userData.UserSettings.ToshlFullSyncStartedAt = null;
            userData.UserSettings.ToshlFullSyncCompletedAt = null;
            userData.UserSettings.ToshlFullSyncError = string.Empty;
            userData.UserSettings.ToshlFullSyncProgressPercent = 0;
            userData.UserSettings.ToshlFullSyncProgressDescription = string.Empty;
        }
        await userDataContext.SaveChangesAsync();
    }

    public async Task RemoveAccessTokenAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        userData.ToshlAccessToken = string.Empty;
        userData.ToshlLastSync = DateTime.MinValue;
        if (userData.UserSettings != null)
        {
            userData.UserSettings.ToshlFullSyncStatus = ToshlFullSyncStatuses.Idle;
            userData.UserSettings.ToshlFullSyncQueuedAt = null;
            userData.UserSettings.ToshlFullSyncStartedAt = null;
            userData.UserSettings.ToshlFullSyncCompletedAt = null;
            userData.UserSettings.ToshlFullSyncError = string.Empty;
            userData.UserSettings.ToshlFullSyncProgressPercent = 0;
            userData.UserSettings.ToshlFullSyncProgressDescription = string.Empty;
        }
        await userDataContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<string>> SyncAsync(
        Guid userGuid,
        bool force = false,
        bool trackFullSyncProgress = false
    )
    {
        var syncLock = s_userSyncLocks.GetOrAdd(userGuid, _ => new SemaphoreSlim(1, 1));
        if (!await syncLock.WaitAsync(0))
        {
            throw new BudgetBoardServiceException(
                "A Toshl sync is already in progress for this account. Wait for it to finish and try again.",
                (int)HttpStatusCode.Conflict
            );
        }

        try
        {
        var userData = await GetCurrentUserAsync(userGuid);
        if (trackFullSyncProgress)
        {
            await UpdateFullSyncProgressAsync(userGuid, 5, "Connecting to Toshl");
        }

        if (string.IsNullOrWhiteSpace(userData.ToshlAccessToken))
        {
            logger.LogInformation("Skipping Toshl sync because no access token is configured.");
            return Array.Empty<string>();
        }

        var autoSyncPeriod = userData.UserSettings?.ToshlAutoSyncIntervalHours ?? 8;
        if (!force && !IsSyncDue(userData.ToshlLastSync, autoSyncPeriod, DateTime.UtcNow))
        {
            logger.LogInformation(
                "Skipping Toshl sync because the last sync is still within the configured Toshl auto-sync period."
            );
            return Array.Empty<string>();
        }

        var direction = userData.UserSettings?.ToshlMetadataSyncDirection
            ?? ToshlMetadataSyncDirections.Toshl;
        var errors = new List<string>();

        if (direction == ToshlMetadataSyncDirections.BudgetBoard)
        {
            if (trackFullSyncProgress)
            {
                await UpdateFullSyncProgressAsync(userGuid, 15, "Syncing metadata to Toshl");
            }
            errors.AddRange(await PushLocalMetadataAsync(userData));
        }
        else
        {
            if (trackFullSyncProgress)
            {
                await UpdateFullSyncProgressAsync(userGuid, 15, "Syncing metadata from Toshl");
            }
            errors.AddRange(await PullRemoteMetadataAsync(userData));
        }

        if (trackFullSyncProgress)
        {
            await UpdateFullSyncProgressAsync(userGuid, 30, "Preparing transaction import");
        }

        errors.AddRange(
            await ImportRemoteTransactionsAsync(
                userData,
                fullSync: trackFullSyncProgress,
                trackFullSyncProgress
                    ? (processed, total) =>
                        UpdateFullSyncProgressAsync(
                            userGuid,
                            30 + (int)Math.Round((processed / (double)Math.Max(total, 1)) * 65),
                            $"Importing transactions {processed}/{total}"
                        )
                    : null
            )
        );

        if (trackFullSyncProgress)
        {
            await UpdateFullSyncProgressAsync(userGuid, 98, "Finalizing sync");
        }

        await userDataContext
            .ApplicationUsers.Where(u => u.Id == userGuid)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(u => u.ToshlLastSync, DateTime.UtcNow));

        return errors;
        }
        finally
        {
            syncLock.Release();
        }
    }

    private async Task<IList<string>> PushLocalMetadataAsync(ApplicationUser userData)
    {
        var errors = new List<string>();
        var remoteCategories = await GetRemoteItemsAsync(ToshlCategoriesEndpoint, userData);
        var remoteCategoriesByName = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .ToDictionary(c => c.Name!, StringComparer.OrdinalIgnoreCase);

        var rootCategories = userData
            .TransactionCategories.Where(c => string.IsNullOrEmpty(c.Parent))
            .OrderBy(c => c.Value)
            .ToList();

        foreach (var localCategory in rootCategories)
        {
            var remoteCategory = await CreateOrUpdateRemoteCategoryAsync(
                userData,
                localCategory.Value,
                null,
                remoteCategoriesByName
            );

            await PushChildCategoriesAsync(
                userData,
                localCategory.Value,
                remoteCategory.Id,
                remoteCategories
            );
        }

        return errors;
    }

    private async Task PushChildCategoriesAsync(
        ApplicationUser userData,
        string parentName,
        string? remoteParentId,
        IEnumerable<ToshlMetadataItem> remoteCategories
    )
    {
        var localChildren = userData
            .TransactionCategories.Where(c =>
                c.Parent.Equals(parentName, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(c => c.Value)
            .ToList();

        var remoteByName = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .ToDictionary(c => c.Name!, StringComparer.OrdinalIgnoreCase);

        foreach (var child in localChildren)
        {
            var remoteChild = await CreateOrUpdateRemoteCategoryAsync(
                userData,
                child.Value,
                remoteParentId,
                remoteByName
            );

            await PushChildCategoriesAsync(
                userData,
                child.Value,
                remoteChild.Id,
                remoteCategories
            );
        }
    }

    private async Task<IList<string>> PullRemoteMetadataAsync(ApplicationUser userData)
    {
        var errors = new List<string>();
        var remoteCategories = await GetRemoteItemsAsync(ToshlCategoriesEndpoint, userData);
        var remoteTags = await GetRemoteItemsAsync(ToshlTagsEndpoint, userData);
        var existingCategoryKeys = await userDataContext
            .TransactionCategories.Where(c => c.UserID == userData.Id)
            .Select(c => new { c.Value, c.Parent })
            .AsNoTracking()
            .ToListAsync();
        var categoryKeys = existingCategoryKeys
            .Select(c => BuildCategoryKey(c.Value, c.Parent))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var categoriesToInsert = new List<Category>();

        var remoteCategoryById = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToDictionary(c => c.Id!, c => c, StringComparer.OrdinalIgnoreCase);

        foreach (var remoteCategory in remoteCategories.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
        {
            var parentName = string.Empty;
            if (
                !string.IsNullOrWhiteSpace(remoteCategory.Category)
                && remoteCategoryById.TryGetValue(remoteCategory.Category!, out var parentCategory)
                && !string.IsNullOrWhiteSpace(parentCategory.Name)
            )
            {
                parentName = parentCategory.Name!;
            }

            UpsertLocalCategory(userData.Id, remoteCategory.Name!, parentName, categoryKeys, categoriesToInsert);
        }

        var tagRootName = "Toshl Tags";
        UpsertLocalCategory(userData.Id, tagRootName, string.Empty, categoryKeys, categoriesToInsert);

        foreach (var remoteTag in remoteTags.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
        {
            UpsertLocalCategory(userData.Id, remoteTag.Name!, tagRootName, categoryKeys, categoriesToInsert);
        }

        if (categoriesToInsert.Count > 0)
        {
            await userDataContext.TransactionCategories.AddRangeAsync(categoriesToInsert);
            await userDataContext.SaveChangesAsync();
        }

        return errors;
    }

    private async Task<ToshlMetadataItem> CreateOrUpdateRemoteCategoryAsync(
        ApplicationUser userData,
        string name,
        string? parentId,
        IDictionary<string, ToshlMetadataItem> remoteByName
    )
    {
        if (remoteByName.TryGetValue(name, out var existing))
        {
            if (existing.Id is not null)
            {
                await UpdateRemoteCategoryAsync(userData, existing.Id, name, parentId);
            }

            return existing with { Category = parentId };
        }

        var created = await CreateRemoteCategoryAsync(userData, name, parentId);
        remoteByName[name] = created;
        return created;
    }

    private async Task<ToshlMetadataItem> CreateRemoteCategoryAsync(
        ApplicationUser userData,
        string name,
        string? parentId
    )
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["type"] = "expense",
        };
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            payload["category"] = parentId;
        }

        var response = await SendToshlRequestAsync(
            userData,
            HttpMethod.Post,
            ToshlCategoriesEndpoint,
            payload
        );
        var body = await response.Content.ReadAsStringAsync();
        return DeserializeMetadataItem(body, name, parentId);
    }

    private async Task UpdateRemoteCategoryAsync(
        ApplicationUser userData,
        string remoteId,
        string name,
        string? parentId
    )
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["type"] = "expense",
        };
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            payload["category"] = parentId;
        }

        await SendToshlRequestAsync(
            userData,
            HttpMethod.Put,
            $"{ToshlCategoriesEndpoint}/{remoteId}",
            payload
        );
    }

    private static string BuildCategoryKey(string value, string parent)
    {
        return $"{parent}::{value}";
    }

    private static void UpsertLocalCategory(
        Guid userGuid,
        string value,
        string parent,
        ISet<string> existingKeys,
        ICollection<Category> categoriesToInsert
    )
    {
        var key = BuildCategoryKey(value, parent);
        if (!existingKeys.Add(key))
        {
            return;
        }

        categoriesToInsert.Add(
            new Category
            {
                Value = value,
                Parent = parent,
                UserID = userGuid,
            }
        );
    }

    private async Task<IReadOnlyList<ToshlMetadataItem>> GetRemoteItemsAsync(
        string endpoint,
        ApplicationUser userData
    )
    {
        var response = await SendToshlRequestAsync(userData, HttpMethod.Get, endpoint, null);
        var body = await response.Content.ReadAsStringAsync();
        return DeserializeMetadataList(body);
    }

    private async Task<HttpResponseMessage> SendToshlRequestAsync(
        ApplicationUser userData,
        HttpMethod method,
        string endpoint,
        object? payload
    )
    {
        var client = clientFactory.CreateClient(nameof(ToshlService));
        var request = new HttpRequestMessage(method, ToshlBaseUrl + endpoint);
        var authToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{userData.ToshlAccessToken}:")
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        if (payload != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload, s_jsonOptions),
                Encoding.UTF8,
                "application/json"
            );
        }

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            var errorMessage = BuildToshlErrorMessage(endpoint, response.StatusCode, body);
            logger.LogError(
                "Toshl request to {Endpoint} failed with {StatusCode}: {Body}",
                endpoint,
                response.StatusCode,
                body
            );
            throw new BudgetBoardServiceException(errorMessage, (int)response.StatusCode);
        }

        return response;
    }

    private static string BuildToshlErrorMessage(string endpoint, HttpStatusCode statusCode, string body)
    {
        var normalizedBody = NormalizeToshlErrorBody(body);
        return string.IsNullOrWhiteSpace(normalizedBody)
            ? $"Toshl request to {endpoint} failed with status {(int)statusCode} ({statusCode})."
            : $"Toshl request to {endpoint} failed with status {(int)statusCode} ({statusCode}). Toshl response: {normalizedBody}";
    }

    private static string NormalizeToshlErrorBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            return body.Trim();
        }
    }

    private static IReadOnlyList<ToshlMetadataItem> DeserializeMetadataList(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<ToshlMetadataItem>();
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<ToshlMetadataItem>>(body, s_jsonOptions)
                ?? new List<ToshlMetadataItem>();
        }

        foreach (var propertyName in new[] { "data", "items", "results" })
        {
            if (
                document.RootElement.TryGetProperty(propertyName, out var listElement)
                && listElement.ValueKind == JsonValueKind.Array
            )
            {
                return JsonSerializer.Deserialize<List<ToshlMetadataItem>>(
                    listElement.GetRawText(),
                    s_jsonOptions
                ) ?? new List<ToshlMetadataItem>();
            }
        }

        return Array.Empty<ToshlMetadataItem>();
    }

    private static ToshlMetadataItem DeserializeMetadataItem(
        string body,
        string fallbackName,
        string? fallbackParentId
    )
    {
        var item = JsonSerializer.Deserialize<ToshlMetadataItem>(body, s_jsonOptions);
        return item
            ?? new ToshlMetadataItem
            {
                Name = fallbackName,
                Category = fallbackParentId,
            };
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid userGuid)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.TransactionCategories)
                .Include(u => u.UserSettings)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .FirstOrDefaultAsync(u => u.Id == userGuid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UserRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }

    private Task UpdateFullSyncProgressAsync(Guid userGuid, int percent, string description)
    {
        return userDataContext
            .UserSettings.Where(us => us.UserID == userGuid)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(
                        us => us.ToshlFullSyncProgressPercent,
                        Math.Clamp(percent, 0, 100)
                    )
                    .SetProperty(
                        us => us.ToshlFullSyncProgressDescription,
                        description ?? string.Empty
                    )
            );
    }

    private sealed record ToshlMetadataItem
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? Type { get; init; }
        public string? Category { get; init; }
        public string? Modified { get; init; }
        public bool? Deleted { get; init; }
    }

}
