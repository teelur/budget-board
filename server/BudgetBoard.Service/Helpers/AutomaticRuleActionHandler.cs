using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Service.Helpers;

internal static class AutomaticRuleActionHandler
{
    internal static async Task<int> ApplyActionToTransactions(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        ITransactionService transactionService,
        Guid userGuid,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Delete,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            await transactionService.DeleteTransactionBatchAsync(
                userGuid,
                transactions.Select(t => t.ID)
            );
            return transactions.Count();
        }
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Set,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplySetAction(
                action,
                transactions,
                allCategories,
                transactionService,
                userGuid,
                responseLocalizer
            );
        }

        throw new BudgetBoardServiceException(
            responseLocalizer["AutomaticRuleUnsupportedOperatorError", action.Operator]
        );
    }

    private static async Task<int> ApplySetAction(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        ITransactionService transactionService,
        Guid userGuid,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Merchant,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplySetActionForMerchant(
                action,
                transactions,
                transactionService,
                userGuid
            );
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Category,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplySetActionForCategory(
                action,
                transactions,
                allCategories,
                transactionService,
                userGuid,
                responseLocalizer
            );
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Amount,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplySetActionForAmount(
                action,
                transactions,
                transactionService,
                userGuid,
                responseLocalizer
            );
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Date,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplySetActionForDate(
                action,
                transactions,
                transactionService,
                userGuid,
                responseLocalizer
            );
        }
        else
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleUnsupportedActionFieldError", action.Field]
            );
        }
    }

    private static async Task<int> ApplySetActionForMerchant(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        var updatedTransactions = 0;
        foreach (var transaction in transactions)
        {
            await transactionService.UpdateTransactionAsync(
                userGuid,
                new TransactionUpdateRequest(transaction) { MerchantName = action.Value }
            );

            updatedTransactions++;
        }
        return updatedTransactions;
    }

    private static async Task<int> ApplySetActionForCategory(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        ITransactionService transactionService,
        Guid userGuid,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        string newCategory = string.Empty;

        if (!string.IsNullOrEmpty(action.Value))
        {
            var foundCategory = allCategories
                .FirstOrDefault(c =>
                    c.Value.Equals(action.Value, StringComparison.CurrentCultureIgnoreCase)
                )
                ?.Value;

            if (foundCategory != null)
            {
                newCategory = foundCategory;
            }
            else
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutomaticRuleCategoryNotFoundError", action.Value]
                );
            }
        }

        int updatedTransactions = 0;
        foreach (var transaction in transactions)
        {
            var updateRequest = new TransactionUpdateRequest(transaction);
            (updateRequest.Category, updateRequest.Subcategory) =
                TransactionCategoriesHelpers.GetFullCategory(newCategory, allCategories);
            await transactionService.UpdateTransactionAsync(userGuid, updateRequest);

            updatedTransactions++;
        }
        return updatedTransactions;
    }

    private static async Task<int> ApplySetActionForAmount(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (!decimal.TryParse(action.Value, out var newAmount))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidAmountError", action.Value]
            );
        }

        int updatedTransactions = 0;
        foreach (var transaction in transactions)
        {
            await transactionService.UpdateTransactionAsync(
                userGuid,
                new TransactionUpdateRequest(transaction) { Amount = newAmount }
            );

            updatedTransactions++;
        }
        return updatedTransactions;
    }

    private static async Task<int> ApplySetActionForDate(
        IRuleParameterRequest action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (!DateOnly.TryParse(action.Value, out var newDate))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidDateError", action.Value]
            );
        }

        int updatedTransactions = 0;
        foreach (var transaction in transactions)
        {
            await transactionService.UpdateTransactionAsync(
                userGuid,
                new TransactionUpdateRequest(transaction) { Date = newDate }
            );

            updatedTransactions++;
        }
        return updatedTransactions;
    }
}
