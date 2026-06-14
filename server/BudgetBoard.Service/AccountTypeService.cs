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

        ValidateAccountTypeData(
            request.Value,
            request.Parent,
            request.Classification,
            allAccountTypes
        );

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

        ValidateAccountTypeData(
            request.Value,
            request.Parent,
            request.Classification,
            allAccountTypes.Where(a => a.ID != request.ID)
        );

        var oldValue = accountType.Value;

        accountType.Value = request.Value;
        accountType.Parent = request.Parent;
        accountType.Classification = ResolveClassification(
            request.Parent,
            request.Classification,
            allAccountTypes
        );

        UpdateAccountsUsingType(userData.Accounts, oldValue, request.Value);

        UpdateChildrenClassification(userData.AccountTypes, oldValue, accountType.Classification);
        UpdateChildrenParentValue(userData.AccountTypes, oldValue, request.Value);

        await userDataContext.SaveChangesAsync();

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
        UpdateAccountsUsingType(userData.Accounts, accountType.Value, string.Empty);

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
                UpdateAccountsUsingType(userData.Accounts, child.Value, string.Empty);
                userData.AccountTypes.Remove(child);
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
                users
                    .Include(u => u.AccountTypes)
                    .Include(u => u.Accounts)
                    .Include(u => u.UserSettings)
        );
    }

    private void ValidateAccountTypeData(
        string value,
        string parent,
        string classification,
        IEnumerable<IAccountTypeResponse> allAccountTypes
    )
    {
        ThrowIfValueIsNullOrEmpty(value);
        ThrowIfValueAlreadyExists(value, allAccountTypes);
        ThrowIfValueSameNameAsParent(value, parent, allAccountTypes);
        ThrowIfParentNotFound(parent, allAccountTypes);
        ThrowIfInvalidClassification(classification);

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

        void ThrowIfInvalidClassification(string classification)
        {
            if (!AccountTypeClassification.IsValid(classification))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["AccountTypeInvalidClassificationLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeInvalidClassificationError"]
                );
            }
        }
    }

    private static string ResolveClassification(
        string parent,
        string classification,
        IEnumerable<IAccountTypeResponse> allAccountTypes
    ) =>
        string.IsNullOrEmpty(parent)
            ? classification
            : allAccountTypes
                .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                .Classification;

    private static void UpdateAccountsUsingType(
        ICollection<Account> accounts,
        string oldType,
        string newType
    )
    {
        foreach (var account in accounts)
        {
            if (account.Type.Equals(oldType, StringComparison.OrdinalIgnoreCase))
            {
                account.Type = newType;
            }
        }
    }
}
