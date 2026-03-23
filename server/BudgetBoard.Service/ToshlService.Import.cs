using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public partial class ToshlService
{
    private const int ToshlIncrementalOverlapDays = 14;
    private const int ToshlInitialIncrementalLookbackDays = 30;

    private async Task<IReadOnlyList<string>> ImportRemoteTransactionsAsync(
        ApplicationUser userData,
        bool fullSync,
        Func<int, int, Task>? progressCallback = null
    )
    {
        var errors = new List<string>();

        var remoteAccounts = await GetRemoteItemsAsync<ToshlAccountItem>(
            ToshlAccountsEndpoint,
            userData
        );
        var remoteCategories = await GetRemoteItemsAsync<ToshlMetadataItem>(
            ToshlCategoriesEndpoint,
            userData
        );
        var remoteTags = await GetRemoteItemsAsync<ToshlTagItem>(ToshlTagsEndpoint, userData);
        var entriesEndpoint = BuildToshlEntriesEndpoint(
            ToshlEntriesEndpoint,
            userData.UserSettings?.ToshlSyncLookbackMonths ?? 0,
            userData.ToshlLastSync,
            fullSync
        );
        var remoteEntries = await GetRemoteItemsAsync<ToshlEntryItem>(
            entriesEndpoint,
            userData
        );

        var accountMap = await EnsureToshlImportAccountsAsync(userData, remoteAccounts);
        errors.AddRange(await SyncRemoteBalancesAsync(userData, remoteAccounts, accountMap));
        var resolvedMappings = BuildResolvedMappings(userData, remoteCategories, remoteTags);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var categoryNamesById = remoteCategories
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToDictionary(c => c.Id!, c => c.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var tagNamesById = remoteTags
            .Where(t => !string.IsNullOrWhiteSpace(t.Id))
            .ToDictionary(t => t.Id!, t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        errors.AddRange(
            await ImportRemoteBudgetsAsync(
                userData,
                categoryNamesById,
                tagNamesById,
                resolvedMappings,
                allCategories
            )
        );

        userDataContext.ChangeTracker.Clear();

        var localTransactionsBySyncId = await userDataContext
            .Transactions.AsNoTracking()
            .Where(t =>
                t.Account != null
                && t.Account.UserID == userData.Id
                && t.Account.Institution != null
                && EF.Functions.ILike(t.Account.Institution.Name, ToshlImportInstitutionName)
                && !string.IsNullOrWhiteSpace(t.SyncID)
                && t.Deleted == null
            )
            .Select(t => new ToshlExistingTransaction
            {
                ID = t.ID,
                SyncID = t.SyncID!,
                AccountID = t.AccountID,
                Amount = t.Amount,
                Date = t.Date,
                MerchantName = t.MerchantName,
                Category = t.Category,
                Subcategory = t.Subcategory,
            })
            .ToDictionaryAsync(t => t.SyncID, StringComparer.OrdinalIgnoreCase);

        var transactionImports = new List<TransactionImport>();
        var processedEntries = 0;
        var totalEntries = remoteEntries.Count;

        foreach (var entry in remoteEntries)
        {
            processedEntries++;
            if (!string.IsNullOrWhiteSpace(entry.Id)
                && localTransactionsBySyncId.TryGetValue(entry.Id!, out var existingTransaction)
                && entry.Deleted == true)
            {
                await transactionService.DeleteTransactionAsync(userData.Id, existingTransaction.ID);
            }
            else
            {
                if (
                    string.IsNullOrWhiteSpace(entry.Account)
                    || !accountMap.TryGetValue(entry.Account!, out var mappedAccount)
                )
                {
                    logger.LogInformation(
                        "Skipping Toshl entry {EntryId} because account '{Account}' could not be mapped.",
                        entry.Id,
                        entry.Account
                    );
                }
                else
                {
                    var merchantName = ResolveToshlEntryMerchantName(entry, tagNamesById);
                    var resolvedCategoryName = ResolveToshlEntryCategory(
                        entry,
                        categoryNamesById,
                        tagNamesById,
                        resolvedMappings
                    );

                    if (
                        !string.IsNullOrWhiteSpace(entry.Id)
                        && localTransactionsBySyncId.TryGetValue(entry.Id!, out existingTransaction)
                    )
                    {
                        await UpsertExistingToshlTransactionAsync(
                            userData,
                            existingTransaction,
                            entry,
                            mappedAccount,
                            merchantName,
                            resolvedCategoryName,
                            allCategories
                        );
                    }
                    else if (entry.Deleted != true)
                    {
                        transactionImports.Add(
                            new TransactionImport
                            {
                                SyncID = entry.Id,
                                Date = ParseToshlDate(entry.Date),
                                MerchantName = merchantName,
                                Category = resolvedCategoryName,
                                Amount = NormalizeToshlAmount(entry),
                                Account = mappedAccount.Name,
                            }
                        );
                    }
                }
            }

            if (
                progressCallback != null
                && (
                    processedEntries == totalEntries
                    || processedEntries == 1
                    || processedEntries % 100 == 0
                )
            )
            {
                await progressCallback(processedEntries, totalEntries);
            }
        }

        if (transactionImports.Count == 0)
        {
            return errors;
        }

        try
        {
            await transactionService.ImportTransactionsAsync(
                userData.Id,
                new TransactionImportRequest
                {
                    Transactions = transactionImports,
                    AccountNameToIDMap = accountMap.Values
                        .Select(account => new AccountNameToIDKeyValuePair
                        {
                            AccountName = account.Name,
                            AccountID = account.Id,
                        })
                        .ToList(),
                }
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            logger.LogError("Toshl transaction import failed: {Message}", bbex.Message);
            errors.Add(bbex.Message);
        }

        return errors;
    }

    private async Task UpsertExistingToshlTransactionAsync(
        ApplicationUser userData,
        ToshlExistingTransaction existingTransaction,
        ToshlEntryItem entry,
        ToshlImportedAccount mappedAccount,
        string merchantName,
        string resolvedCategoryName,
        IEnumerable<ICategory> allCategories
    )
    {
        var normalizedAmount = NormalizeToshlAmount(entry) ?? existingTransaction.Amount;
        var normalizedDate = ParseToshlDate(entry.Date);

        if (existingTransaction.AccountID != mappedAccount.Id)
        {
            await transactionService.DeleteTransactionAsync(userData.Id, existingTransaction.ID);
            await transactionService.CreateTransactionAsync(
                userData.Id,
                BuildToshlTransactionCreateRequest(
                    entry,
                    mappedAccount.Id,
                    normalizedAmount,
                    normalizedDate,
                    merchantName,
                    resolvedCategoryName,
                    allCategories
                ),
                allCategories
            );
            return;
        }

        var updateRequest = new TransactionUpdateRequest
        {
            ID = existingTransaction.ID,
            Category = existingTransaction.Category,
            Subcategory = existingTransaction.Subcategory,
            Deleted = null,
            Amount = normalizedAmount,
            Date = normalizedDate,
            MerchantName = !string.IsNullOrWhiteSpace(merchantName)
                ? merchantName
                : existingTransaction.MerchantName,
        };

        ApplyResolvedCategoryToRequest(updateRequest, resolvedCategoryName, allCategories);

        var hasChanged =
            updateRequest.Amount != existingTransaction.Amount
            || updateRequest.Date != existingTransaction.Date
            || !string.Equals(
                updateRequest.MerchantName,
                existingTransaction.MerchantName,
                StringComparison.Ordinal
            )
            || !string.Equals(updateRequest.Category, existingTransaction.Category, StringComparison.Ordinal)
            || !string.Equals(
                updateRequest.Subcategory,
                existingTransaction.Subcategory,
                StringComparison.Ordinal
            );

        if (hasChanged)
        {
            await transactionService.UpdateTransactionAsync(userData.Id, updateRequest);
        }
    }

    private static TransactionCreateRequest BuildToshlTransactionCreateRequest(
        ToshlEntryItem entry,
        Guid accountId,
        decimal normalizedAmount,
        DateTime normalizedDate,
        string merchantName,
        string resolvedCategoryName,
        IEnumerable<ICategory> allCategories
    )
    {
        var request = new TransactionCreateRequest
        {
            SyncID = entry.Id,
            Amount = normalizedAmount,
            Date = normalizedDate,
            MerchantName = merchantName,
            Source = TransactionSource.Manual.Value,
            AccountID = accountId,
        };
        ApplyResolvedCategoryToRequest(request, resolvedCategoryName, allCategories);
        return request;
    }

    private static void ApplyResolvedCategoryToRequest(
        ITransactionUpdateRequest request,
        string resolvedCategoryName,
        IEnumerable<ICategory> allCategories
    )
    {
        if (request is not TransactionUpdateRequest updateRequest)
        {
            return;
        }

        var matchedCategory =
            allCategories
                .FirstOrDefault(c =>
                    c.Value.Equals(resolvedCategoryName, StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(matchedCategory))
        {
            return;
        }

        var (parent, child) = TransactionCategoriesHelpers.GetFullCategory(
            matchedCategory,
            allCategories
        );
        updateRequest.Category = parent;
        updateRequest.Subcategory = child;
    }

    private static void ApplyResolvedCategoryToRequest(
        TransactionCreateRequest request,
        string resolvedCategoryName,
        IEnumerable<ICategory> allCategories
    )
    {
        var matchedCategory =
            allCategories
                .FirstOrDefault(c =>
                    c.Value.Equals(resolvedCategoryName, StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(matchedCategory))
        {
            return;
        }

        var (parent, child) = TransactionCategoriesHelpers.GetFullCategory(
            matchedCategory,
            allCategories
        );
        request.Category = parent;
        request.Subcategory = child;
    }

    private async Task<Dictionary<string, ToshlImportedAccount>> EnsureToshlImportAccountsAsync(
        ApplicationUser userData,
        IReadOnlyList<ToshlAccountItem> remoteAccounts
    )
    {
        var accountMap = await userDataContext
            .Accounts.Where(a => a.UserID == userData.Id && a.Deleted == null)
            .ToDictionaryAsync(a => a.Name, StringComparer.OrdinalIgnoreCase);

        var institution = await userDataContext.Institutions.FirstOrDefaultAsync(i =>
            i.UserID == userData.Id
            && EF.Functions.ILike(i.Name, ToshlImportInstitutionName)
        );
        if (institution == null)
        {
            var institutionCount = await userDataContext.Institutions.CountAsync(i =>
                i.UserID == userData.Id
            );
            institution = new Institution
            {
                Name = ToshlImportInstitutionName,
                UserID = userData.Id,
                Index = institutionCount,
            };
            userDataContext.Institutions.Add(institution);
        }
        else if (institution.Deleted != null)
        {
            institution.Deleted = null;
        }

        var mappedAccounts = new Dictionary<string, ToshlImportedAccount>(
            StringComparer.OrdinalIgnoreCase
        );
        var modifiedAnything = false;

        foreach (var remoteAccount in remoteAccounts.Where(a => !string.IsNullOrWhiteSpace(a.Name)))
        {
            var key = remoteAccount.Id ?? remoteAccount.Name!;
            if (accountMap.TryGetValue(remoteAccount.Name!, out var existingAccount))
            {
                if (
                    existingAccount.Deleted != null
                    || existingAccount.InstitutionID != institution.ID
                    || !string.Equals(existingAccount.Source, AccountSource.Toshl, StringComparison.Ordinal)
                )
                {
                    existingAccount.Deleted = null;
                    existingAccount.Institution = institution;
                    existingAccount.InstitutionID = institution.ID;
                    existingAccount.Source = AccountSource.Toshl;
                    modifiedAnything = true;
                }

                mappedAccounts[key] = new ToshlImportedAccount(
                    existingAccount.ID,
                    existingAccount.Name
                );
                continue;
            }

            var createdAccount = new Account
            {
                Name = remoteAccount.Name!,
                Institution = institution,
                InstitutionID = institution.ID,
                Type = "Other",
                Subtype = string.Empty,
                HideTransactions = false,
                HideAccount = false,
                Source = AccountSource.Toshl,
                UserID = userData.Id,
                Index = accountMap.Count,
            };

            userDataContext.Accounts.Add(createdAccount);
            accountMap[createdAccount.Name] = createdAccount;
            mappedAccounts[key] = new ToshlImportedAccount(createdAccount.ID, createdAccount.Name);
            modifiedAnything = true;
        }

        if (modifiedAnything)
        {
            userDataContext.Entry(userData).State = EntityState.Unchanged;
            if (userData.UserSettings != null)
            {
                userDataContext.Entry(userData.UserSettings).State = EntityState.Unchanged;
            }
            foreach (var categoryEntry in userDataContext.ChangeTracker.Entries<Category>())
            {
                if (categoryEntry.State == EntityState.Modified)
                {
                    categoryEntry.State = EntityState.Unchanged;
                }
            }
            await userDataContext.SaveChangesAsync();
        }

        return mappedAccounts;
    }

    private async Task<IReadOnlyList<string>> SyncRemoteBalancesAsync(
        ApplicationUser userData,
        IReadOnlyList<ToshlAccountItem> remoteAccounts,
        IReadOnlyDictionary<string, ToshlImportedAccount> accountMap
    )
    {
        var nowUtc = DateTime.UtcNow;

        foreach (var remoteAccount in remoteAccounts.Where(a =>
                     !string.IsNullOrWhiteSpace(a.Name)
                     && !string.IsNullOrWhiteSpace(a.Id)
                     && a.Deleted != true
                     && a.Balance.HasValue
                 ))
        {
            if (!accountMap.TryGetValue(remoteAccount.Id!, out var mappedAccount))
            {
                continue;
            }

            var existingBalanceForToday = await userDataContext.Balances.FirstOrDefaultAsync(b =>
                b.AccountID == mappedAccount.Id
                && b.Deleted == null
                && b.DateTime.Date == nowUtc.Date
            );
            if (existingBalanceForToday != null)
            {
                existingBalanceForToday.Amount = remoteAccount.Balance!.Value;
                existingBalanceForToday.DateTime = nowUtc;
            }
            else
            {
                userDataContext.Balances.Add(
                    new Balance
                    {
                        AccountID = mappedAccount.Id,
                        Amount = remoteAccount.Balance!.Value,
                        DateTime = nowUtc,
                    }
                );
            }
        }

        await userDataContext.SaveChangesAsync();

        return Array.Empty<string>();
    }

    private async Task<IReadOnlyList<string>> ImportRemoteBudgetsAsync(
        ApplicationUser userData,
        IReadOnlyDictionary<string, string> categoryNamesById,
        IReadOnlyDictionary<string, string> tagNamesById,
        IReadOnlyDictionary<string, string> resolvedMappings,
        IReadOnlyList<ICategory> allCategories
    )
    {
        var currentMonthStart = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
        var remoteBudgets = await GetRemoteItemsAsync<ToshlBudgetItem>(
            $"{ToshlBudgetsEndpoint}?from={currentMonthStart:yyyy-MM-dd}&to={currentMonthEnd:yyyy-MM-dd}&include_deleted=true&one_iteration_only=true",
            userData
        );
        if (remoteBudgets.Count == 0)
        {
            return Array.Empty<string>();
        }

        var latestBudgets = remoteBudgets
            .Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Deleted != true && b.Limit.HasValue)
            .ToList();

        var resolvedBudgetCategories = latestBudgets
            .Select(remoteBudget => new
            {
                RemoteBudget = remoteBudget,
                Category = ResolveToshlBudgetCategory(
                    remoteBudget,
                    categoryNamesById,
                    tagNamesById,
                    resolvedMappings,
                    allCategories
                ),
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Category))
            .ToList();

        var validCategories = resolvedBudgetCategories
            .Select(x => x.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var staleBudgets = await userDataContext.Budgets
            .Where(b =>
                b.UserID == userData.Id
                && b.Date.Year == currentMonthStart.Year
                && b.Date.Month == currentMonthStart.Month
                && !validCategories.Contains(b.Category)
            )
            .ToListAsync();

        if (staleBudgets.Count > 0)
        {
            userDataContext.Budgets.RemoveRange(staleBudgets);
        }

        foreach (var budgetResolution in resolvedBudgetCategories)
        {
            var remoteBudget = budgetResolution.RemoteBudget;
            var mappedCategory = budgetResolution.Category;

            var existingBudget = await userDataContext.Budgets.FirstOrDefaultAsync(b =>
                b.UserID == userData.Id
                && b.Date.Year == currentMonthStart.Year
                && b.Date.Month == currentMonthStart.Month
                && b.Category == mappedCategory
            );
            if (existingBudget == null)
            {
                userDataContext.Budgets.Add(
                    new Budget
                    {
                        UserID = userData.Id,
                        Date = currentMonthStart,
                        Category = mappedCategory,
                        Limit = remoteBudget.Limit!.Value,
                    }
                );
                continue;
            }

            existingBudget.Limit = remoteBudget.Limit!.Value;
        }

        await userDataContext.SaveChangesAsync();

        return Array.Empty<string>();
    }

    private static DateTime ParseToshlDate(string? date)
    {
        if (
            !string.IsNullOrWhiteSpace(date)
            && DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var parsed
            )
        )
        {
            return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
        }

        return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    }

    private static string ResolveToshlEntryCategory(
        ToshlEntryItem entry,
        IReadOnlyDictionary<string, string> categoryNamesById,
        IReadOnlyDictionary<string, string> tagNamesById,
        IReadOnlyDictionary<string, string> resolvedMappings
    )
    {
        var firstTagId = entry.Tags?.FirstOrDefault();
        if (
            !string.IsNullOrWhiteSpace(firstTagId)
            && tagNamesById.TryGetValue(firstTagId!, out var tagName)
        )
        {
            var mappingKey = BuildToshlMappingLookupKey(ToshlTagType, firstTagId!);
            if (resolvedMappings.TryGetValue(mappingKey, out var mappedCategory))
            {
                return mappedCategory;
            }
        }

        if (!string.IsNullOrWhiteSpace(entry.Category))
        {
            if (categoryNamesById.TryGetValue(entry.Category!, out var categoryName))
            {
                var mappingKey = BuildToshlMappingLookupKey(ToshlCategoryType, entry.Category!);
                if (resolvedMappings.TryGetValue(mappingKey, out var mappedCategory))
                {
                    return mappedCategory;
                }

                return categoryName;
            }
        }

        if (
            !string.IsNullOrWhiteSpace(firstTagId)
            && tagNamesById.TryGetValue(firstTagId!, out var fallbackTagName)
        )
        {
            return fallbackTagName;
        }

        return string.Empty;
    }

    private static string ResolveMappedToshlCategoryName(
        string toshlCategoryId,
        IReadOnlyDictionary<string, string> categoryNamesById,
        IReadOnlyDictionary<string, string> resolvedMappings
    )
    {
        var mappingKey = BuildToshlMappingLookupKey(ToshlCategoryType, toshlCategoryId);
        if (resolvedMappings.TryGetValue(mappingKey, out var mappedCategory))
        {
            return mappedCategory;
        }

        if (categoryNamesById.TryGetValue(toshlCategoryId, out var categoryName))
        {
            return categoryName;
        }

        return string.Empty;
    }

    private static string ResolveMappedToshlTagCategoryName(
        string toshlTagId,
        IReadOnlyDictionary<string, string> tagNamesById,
        IReadOnlyDictionary<string, string> resolvedMappings
    )
    {
        var mappingKey = BuildToshlMappingLookupKey(ToshlTagType, toshlTagId);
        if (resolvedMappings.TryGetValue(mappingKey, out var mappedCategory))
        {
            return mappedCategory;
        }

        if (tagNamesById.TryGetValue(toshlTagId, out var tagName))
        {
            return tagName;
        }

        return string.Empty;
    }

    private static string ResolveToshlBudgetCategory(
        ToshlBudgetItem budget,
        IReadOnlyDictionary<string, string> categoryNamesById,
        IReadOnlyDictionary<string, string> tagNamesById,
        IReadOnlyDictionary<string, string> resolvedMappings,
        IReadOnlyList<ICategory> allCategories
    )
    {
        var categoryFromName = ResolveToshlBudgetCategoryFromName(budget.Name, allCategories);
        if (!string.IsNullOrWhiteSpace(categoryFromName))
        {
            return categoryFromName;
        }

        var mappedCategories = new List<string>();

        if (budget.Categories != null)
        {
            mappedCategories.AddRange(
                budget.Categories
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => ResolveMappedToshlCategoryName(id!, categoryNamesById, resolvedMappings))
                    .Where(category => !string.IsNullOrWhiteSpace(category))
            );
        }

        if (budget.Tags != null)
        {
            mappedCategories.AddRange(
                budget.Tags
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => ResolveMappedToshlTagCategoryName(id!, tagNamesById, resolvedMappings))
                    .Where(category => !string.IsNullOrWhiteSpace(category))
            );
        }

        mappedCategories = mappedCategories
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (mappedCategories.Count == 1)
        {
            return mappedCategories[0];
        }

        if (mappedCategories.Count > 1)
        {
            var resolvedParents = mappedCategories
                .Select(category => TransactionCategoriesHelpers.GetFullCategory(category, allCategories))
                .Select(fullCategory => string.IsNullOrWhiteSpace(fullCategory.child) ? fullCategory.parent : fullCategory.parent)
                .Where(parent => !string.IsNullOrWhiteSpace(parent))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (resolvedParents.Count == 1)
            {
                return resolvedParents[0];
            }

            return mappedCategories[0];
        }

        return string.Empty;
    }

    private static string ResolveToshlBudgetCategoryFromName(
        string? budgetName,
        IReadOnlyList<ICategory> allCategories
    )
    {
        if (string.IsNullOrWhiteSpace(budgetName))
        {
            return string.Empty;
        }

        var exactCategory = allCategories.FirstOrDefault(c =>
            c.Value.Equals(budgetName, StringComparison.OrdinalIgnoreCase)
        );
        if (exactCategory != null)
        {
            return exactCategory.Value;
        }

        var splitCandidates = budgetName
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        foreach (var candidate in splitCandidates)
        {
            var exactCandidate = allCategories.FirstOrDefault(c =>
                c.Value.Equals(candidate, StringComparison.OrdinalIgnoreCase)
            );
            if (exactCandidate != null)
            {
                return exactCandidate.Value;
            }
        }

        foreach (var candidate in splitCandidates.Prepend(budgetName))
        {
            var suggestedCategory = GetSuggestedBudgetBoardCategory(
                candidate,
                ToshlCategoryType,
                string.Empty,
                allCategories
            );
            if (!string.IsNullOrWhiteSpace(suggestedCategory))
            {
                return suggestedCategory;
            }
        }

        return string.Empty;
    }

    private static string ResolveToshlEntryMerchantName(
        ToshlEntryItem entry,
        IReadOnlyDictionary<string, string> tagNamesById
    )
    {
        if (!string.IsNullOrWhiteSpace(entry.Desc))
        {
            return entry.Desc.Trim();
        }

        if (entry.Tags == null || entry.Tags.Count == 0)
        {
            return string.Empty;
        }

        var tagNames = entry
            .Tags.Where(tagId => !string.IsNullOrWhiteSpace(tagId))
            .Select(tagId => tagNamesById.TryGetValue(tagId!, out var tagName) ? tagName?.Trim() : null)
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return tagNames.Count == 0 ? string.Empty : string.Join(", ", tagNames);
    }

    private static void ApplyResolvedToshlTransactionMetadata(
        Transaction transaction,
        string merchantName,
        string resolvedCategoryName,
        IEnumerable<ICategory> allCategories
    )
    {
        if (!string.IsNullOrWhiteSpace(merchantName))
        {
            transaction.MerchantName = merchantName;
        }

        var matchedCategory =
            allCategories
                .FirstOrDefault(c =>
                    c.Value.Equals(resolvedCategoryName, StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(matchedCategory))
        {
            return;
        }

        var (parent, child) = TransactionCategoriesHelpers.GetFullCategory(
            matchedCategory,
            allCategories
        );
        transaction.Category = parent;
        transaction.Subcategory = child;
    }

    private static decimal? NormalizeToshlAmount(ToshlEntryItem entry)
    {
        if (!entry.Amount.HasValue)
        {
            return null;
        }

        var mainRate = entry.Currency?.MainRate ?? entry.Currency?.Rate;
        if (!mainRate.HasValue || mainRate.Value == 0 || mainRate.Value == 1)
        {
            return entry.Amount.Value;
        }

        return decimal.Round(entry.Amount.Value / mainRate.Value, 2, MidpointRounding.AwayFromZero);
    }

    private static bool IsSyncDue(DateTime lastSyncUtc, int autoSyncPeriod, DateTime nowUtc)
    {
        if (lastSyncUtc == DateTime.MinValue)
        {
            return true;
        }

        return autoSyncPeriod switch
        {
            > 0 => nowUtc - lastSyncUtc >= TimeSpan.FromHours(autoSyncPeriod),
            0 => DateOnly.FromDateTime(nowUtc) > DateOnly.FromDateTime(lastSyncUtc),
            -1 => ISOWeek.GetYear(nowUtc) != ISOWeek.GetYear(lastSyncUtc)
                || ISOWeek.GetWeekOfYear(nowUtc) != ISOWeek.GetWeekOfYear(lastSyncUtc),
            -2 => nowUtc.Year != lastSyncUtc.Year || nowUtc.Month != lastSyncUtc.Month,
            _ => nowUtc - lastSyncUtc >= TimeSpan.FromHours(8),
        };
    }

    private async Task<IReadOnlyList<T>> GetRemoteItemsAsync<T>(
        string endpoint,
        ApplicationUser userData
    )
        where T : class
    {
        var items = new List<T>();
        var page = 0;
        const int perPage = 500;

        while (true)
        {
            var response = await SendToshlRequestAsync(
                userData,
                HttpMethod.Get,
                BuildPagedEndpoint(endpoint, page, perPage),
                null
            );
            var body = await response.Content.ReadAsStringAsync();
            var batch = DeserializeList<T>(body);
            items.AddRange(batch);

            if (batch.Count < perPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private static IReadOnlyList<T> DeserializeList<T>(string body)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<T>();
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<T>>(body, s_jsonOptions) ?? new List<T>();
        }

        foreach (var propertyName in new[] { "data", "items", "results" })
        {
            if (
                document.RootElement.TryGetProperty(propertyName, out var listElement)
                && listElement.ValueKind == JsonValueKind.Array
            )
            {
                return JsonSerializer.Deserialize<List<T>>(listElement.GetRawText(), s_jsonOptions)
                    ?? new List<T>();
            }
        }

        return Array.Empty<T>();
    }

    private static string BuildPagedEndpoint(string endpoint, int page, int perPage)
    {
        var separator = endpoint.Contains('?') ? '&' : '?';
        return $"{endpoint}{separator}page={page}&per_page={perPage}";
    }

    private static string BuildToshlEntriesEndpoint(
        string endpoint,
        int lookbackMonths,
        DateTime lastSyncUtc,
        bool fullSync
    )
    {
        var baseEndpoint = $"{endpoint}?include_deleted=true&expand=false";

        DateTime? fromDate = null;

        if (fullSync)
        {
            if (lookbackMonths > 0)
            {
                fromDate = DateTime.UtcNow.Date.AddMonths(-lookbackMonths);
            }
        }
        else if (lastSyncUtc != DateTime.MinValue)
        {
            fromDate = lastSyncUtc.Date.AddDays(-ToshlIncrementalOverlapDays);
        }
        else if (lookbackMonths > 0)
        {
            fromDate = DateTime.UtcNow.Date.AddMonths(-lookbackMonths);
        }
        else
        {
            fromDate = DateTime.UtcNow.Date.AddDays(-ToshlInitialIncrementalLookbackDays);
        }

        if (!fromDate.HasValue)
        {
            return baseEndpoint;
        }

        var from = fromDate.Value.ToString("yyyy-MM-dd");
        return $"{baseEndpoint}&from={from}";
    }

    private sealed record ToshlImportedAccount(Guid Id, string Name);

    private sealed class ToshlExistingTransaction
    {
        public Guid ID { get; init; }
        public string SyncID { get; init; } = string.Empty;
        public Guid AccountID { get; init; }
        public decimal Amount { get; init; }
        public DateTime Date { get; init; }
        public string? MerchantName { get; init; }
        public string? Category { get; init; }
        public string? Subcategory { get; init; }
    }

    private sealed record ToshlAccountItem
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public decimal? Balance { get; init; }
        public string? Modified { get; init; }
        public bool? Deleted { get; init; }
    }

    private sealed record ToshlTagItem
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? Category { get; init; }
        public string? Modified { get; init; }
        public bool? Deleted { get; init; }
    }

    private sealed record ToshlBudgetItem
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public decimal? Limit { get; init; }
        public IReadOnlyList<string>? Categories { get; init; }
        public IReadOnlyList<string>? Tags { get; init; }
        public IReadOnlyList<string>? Accounts { get; init; }
        public bool? Deleted { get; init; }
    }

    private sealed record ToshlEntryItem
    {
        public string? Id { get; init; }
        public decimal? Amount { get; init; }
        public ToshlEntryCurrencyItem? Currency { get; init; }
        public string? Date { get; init; }
        public string? Desc { get; init; }
        public string? Account { get; init; }
        public string? Category { get; init; }
        public IReadOnlyList<string>? Tags { get; init; }
        public string? Modified { get; init; }
        public bool? Deleted { get; init; }
    }

    private sealed record ToshlEntryCurrencyItem
    {
        public string? Code { get; init; }
        public decimal? Rate { get; init; }
        [JsonPropertyName("main_rate")]
        public decimal? MainRate { get; init; }
    }
}
