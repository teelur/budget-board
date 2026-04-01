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

public class TransactionCategoryService(
    ILogger<ITransactionCategoryService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ITransactionCategoryService
{
    private readonly ILogger<ITransactionCategoryService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateTransactionCategoryAsync(Guid userGuid, ICategoryCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        if (
            allCategories.Any(c =>
                c.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryCreateDuplicateNameLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryCreateDuplicateNameError"]
            );
        }

        if (string.IsNullOrEmpty(request.Value))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryCreateEmptyNameLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryCreateEmptyNameError"]
            );
        }

        if (request.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryCreateSameNameAsParentLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryCreateSameNameAsParentError"]
            );
        }

        if (
            !string.IsNullOrEmpty(request.Parent)
            && !allCategories.Any(c =>
                c.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryCreateParentNotFoundLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryCreateParentNotFoundError"]
            );
        }

        var newCategory = new Category
        {
            Value = request.Value,
            Parent = request.Parent,
            UserID = userData.Id,
        };

        _userDataContext.TransactionCategories.Add(newCategory);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ICategoryResponse>> ReadTransactionCategoriesAsync(
        Guid userGuid,
        Guid categoryGuid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (categoryGuid != default)
        {
            var transactionCategory = userData.TransactionCategories.FirstOrDefault(t =>
                t.ID == categoryGuid
            );
            if (transactionCategory == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["TransactionCategoryNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["TransactionCategoryNotFoundError"]
                );
            }

            return [new CategoryResponse(transactionCategory)];
        }

        return userData.TransactionCategories.Select(c => new CategoryResponse(c)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateTransactionCategoryAsync(Guid userGuid, ICategoryUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t =>
            t.ID == request.ID
        );
        if (transactionCategory == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionCategoryUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryUpdateNotFoundError"]
            );
        }

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        if (
            allCategories.Any(c =>
                c.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryUpdateDuplicateNameLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryUpdateDuplicateNameError"]
            );
        }

        if (string.IsNullOrEmpty(request.Value))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryUpdateEmptyNameLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryUpdateEmptyNameError"]
            );
        }

        if (request.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryUpdateSameNameAsParentLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryUpdateSameNameAsParentError"]
            );
        }

        if (
            !string.IsNullOrEmpty(request.Parent)
            && !allCategories.Any(c =>
                c.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryUpdateParentNotFoundLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryUpdateParentNotFoundError"]
            );
        }

        _userDataContext.Entry(transactionCategory).CurrentValues.SetValues(request);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t => t.ID == guid);
        if (transactionCategory == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionCategoryDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryDeleteNotFoundError"]
            );
        }

        // We want to preserve the category in the database if it is in use.
        var transactionsForUser = userData.Accounts.SelectMany(a => a.Transactions);
        if (
            transactionsForUser.Any(t =>
                (t.Category ?? string.Empty).Equals(
                    transactionCategory.Value,
                    StringComparison.OrdinalIgnoreCase
                )
                || (t.Subcategory ?? string.Empty).Equals(
                    transactionCategory.Value,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryDeleteInUseByTransactionsLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryDeleteInUseByTransactionsError"]
            );
        }
        else if (userData.Budgets.Any(b => b.Category == transactionCategory.Value))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryDeleteInUseByBudgetsLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryDeleteInUseByBudgetsError"]
            );
        }
        else if (userData.TransactionCategories.Any(c => c.Parent == transactionCategory.Value))
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["TransactionCategoryDeleteHasChildrenLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCategoryDeleteHasChildrenError"]
            );
        }
        else
        {
            userData.TransactionCategories.Remove(transactionCategory);
            await _userDataContext.SaveChangesAsync();
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.TransactionCategories)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Budgets)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
