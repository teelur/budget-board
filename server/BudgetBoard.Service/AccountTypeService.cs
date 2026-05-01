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

        if (
            allAccountTypes.Any(a =>
                a.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeCreateDuplicateNameError"]
            );
        }

        if (string.IsNullOrEmpty(request.Value))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateEmptyNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeCreateEmptyNameError"]
            );
        }

        if (request.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateSameNameAsParentLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeCreateSameNameAsParentError"]
            );
        }

        if (
            !string.IsNullOrEmpty(request.Parent)
            && !allAccountTypes.Any(a =>
                a.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeCreateParentNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeCreateParentNotFoundError"]
            );
        }

        var newAccountType = new AccountType
        {
            Value = request.Value,
            Parent = request.Parent,
            UserID = userData.Id,
        };

        userDataContext.AccountTypes.Add(newAccountType);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAccountTypeResponse>> ReadAccountTypesAsync(
        Guid userGuid,
        Guid accountTypeGuid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (accountTypeGuid != default)
        {
            var accountType = userData.AccountTypes.FirstOrDefault(a => a.ID == accountTypeGuid);
            if (accountType == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["AccountTypeNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountTypeNotFoundError"]
                );
            }

            return [new AccountTypeResponse(accountType)];
        }

        return userData.AccountTypes.Select(a => new AccountTypeResponse(a)).ToList();
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

        if (
            allAccountTypes.Any(a =>
                a.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeUpdateDuplicateNameError"]
            );
        }

        if (string.IsNullOrEmpty(request.Value))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateEmptyNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeUpdateEmptyNameError"]
            );
        }

        if (request.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateSameNameAsParentLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeUpdateSameNameAsParentError"]
            );
        }

        if (
            !string.IsNullOrEmpty(request.Parent)
            && !allAccountTypes.Any(a =>
                a.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeUpdateParentNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeUpdateParentNotFoundError"]
            );
        }

        userDataContext.Entry(accountType).CurrentValues.SetValues(request);
        await userDataContext.SaveChangesAsync();
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

        var accountsForUser = userData.Accounts;
        if (
            accountsForUser.Any(a =>
                (a.Type ?? string.Empty).Equals(
                    accountType.Value,
                    StringComparison.OrdinalIgnoreCase
                )
                || (a.Subtype ?? string.Empty).Equals(
                    accountType.Value,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeDeleteInUseByAccountsLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeDeleteInUseByAccountsError"]
            );
        }
        else if (userData.AccountTypes.Any(a => a.Parent == accountType.Value))
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountTypeDeleteHasChildrenLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountTypeDeleteHasChildrenError"]
            );
        }
        else
        {
            userData.AccountTypes.Remove(accountType);
            await userDataContext.SaveChangesAsync();
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
}
