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

public class AccountTypeService(
    ILogger<IAccountTypeService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAccountTypeService
{
    /// <inheritdoc />
    public async Task CreateAccountTypeAsync(Guid userGuid, IAccountTypeCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var allAccountTypes = AccountTypeHelpers.GetAllAccountTypes(userData);

        ThrowIfValueAlreadyExists(request.Value, allAccountTypes);
        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent, allAccountTypes);
        ThrowIfParentNotFound(request.Parent, allAccountTypes);
        ThrowIfInvalidClassification(request.Classification);

        var newAccountType = new AccountType
        {
            Value = request.Value,
            Parent = request.Parent,
            Classification = ResolveClassification(
                request.Parent,
                request.Classification,
                allAccountTypes
            ),
            UserID = userData.Id,
        };

        userDataContext.AccountTypes.Add(newAccountType);
        await userDataContext.SaveChangesAsync();

        void ThrowIfValueAlreadyExists(
            string value,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        )
        {
            if (allAccountTypes.Any(a => a.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateDuplicateNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeCreateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateEmptyNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeCreateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(
            string value,
            string parentValue,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        )
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["AccountTypeCreateSameNameAsParentLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeCreateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(
            string parentValue,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        )
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !allAccountTypes.Any(a =>
                    a.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateParentNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeCreateParentNotFoundError"]
                );
            }
        }

        string ResolveClassification(
            string parent,
            string classification,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        ) =>
            string.IsNullOrEmpty(parent)
                ? classification
                : allAccountTypes
                    .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .Classification;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAccountTypeResponse>> ReadAccountTypesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return AccountTypeHelpers.GetAllAccountTypes(userData);
    }

    /// <inheritdoc />
    public async Task UpdateAccountTypeAsync(Guid userGuid, IAccountTypeUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountType = userData.AccountTypes.FirstOrDefault(a => a.ID == request.ID);
        if (accountType == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeUpdateNotFoundError"]
            );
        }

        var allAccountTypes = AccountTypeHelpers.GetAllAccountTypes(userData);

        ThrowIfValueAlreadyExists(request.Value, allAccountTypes);
        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent);
        ThrowIfParentNotFound(request.Parent);
        ThrowIfInvalidClassification(request.Classification);

        var oldValue = accountType.Value;

        accountType.Value = request.Value;
        accountType.Parent = request.Parent;
        accountType.Classification = ResolveClassification(
            request.Parent,
            request.Classification,
            allAccountTypes
        );

        UpdateAccountsUsingType(userData.Accounts, oldValue, request.Value);

        UpdateChildrenClassification(
            userData.AccountTypes,
            request.Value,
            accountType.Classification
        );
        UpdateChildrenParentValue(userData.AccountTypes, oldValue, request.Value);

        await userDataContext.SaveChangesAsync();

        void ThrowIfValueAlreadyExists(
            string value,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        )
        {
            if (
                allAccountTypes.Any(a =>
                    a.ID != request.ID && a.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateDuplicateNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeUpdateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateEmptyNameLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeUpdateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(string value, string parentValue)
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["AccountTypeUpdateSameNameAsParentLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeUpdateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(string parentValue)
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !allAccountTypes.Any(a =>
                    a.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateParentNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeUpdateParentNotFoundError"]
                );
            }
        }

        string ResolveClassification(
            string parent,
            string classification,
            IEnumerable<IAccountTypeResponse> allAccountTypes
        ) =>
            string.IsNullOrEmpty(parent)
                ? classification
                : allAccountTypes
                    .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .Classification;

        static void UpdateAccountsUsingType(
            ICollection<Account> accounts,
            string oldType,
            string newType
        )
        {
            foreach (var account in accounts)
            {
                if (
                    (account.Type ?? string.Empty).Equals(
                        oldType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    account.Type = newType;
            }
        }

        static void UpdateChildrenClassification(
            ICollection<AccountType> accountTypes,
            string parentValue,
            string newClassification
        )
        {
            foreach (
                var child in accountTypes.Where(a =>
                    a.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.Classification = newClassification;
            }
        }

        static void UpdateChildrenParentValue(
            ICollection<AccountType> accountTypes,
            string oldParentValue,
            string newParentValue
        )
        {
            foreach (
                var child in accountTypes.Where(a =>
                    a.Parent.Equals(oldParentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.Parent = newParentValue;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteAccountTypeAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountType = userData.AccountTypes.FirstOrDefault(a => a.ID == guid);
        if (accountType == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeDeleteNotFoundError"]
            );
        }

        RemoveChildrenUsingType(accountType.Value);
        UpdateAccountsUsingType(userData.Accounts, accountType.Value, null);

        userData.AccountTypes.Remove(accountType);
        await userDataContext.SaveChangesAsync();

        void RemoveChildrenUsingType(string parentValue)
        {
            var children = userData
                .AccountTypes.Where(a =>
                    a.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
            foreach (var child in children)
            {
                UpdateAccountsUsingType(userData.Accounts, child.Value, null);
                userData.AccountTypes.Remove(child);
            }
        }

        static void UpdateAccountsUsingType(
            ICollection<Account> accounts,
            string oldType,
            string? newType
        )
        {
            foreach (var account in accounts)
            {
                if (
                    (account.Type ?? string.Empty).Equals(
                        oldType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    account.Type = newType;
            }
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.AccountTypes)
                .Include(u => u.Accounts)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }

    private void ThrowIfInvalidClassification(string classification)
    {
        if (!AccountClassifications.AllClassifications.Contains(classification))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeInvalidClassificationLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeInvalidClassificationError"]
            );
        }
    }
}
