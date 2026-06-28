using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Service.Helpers;

internal static class AutomaticRuleConditionHandler
{
    internal static IEnumerable<Transaction> FilterOnCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Merchant,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnMerchantCondition(condition, transactions, responseLocalizer);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Category,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnCategoryCondition(
                condition,
                transactions,
                allCategories,
                responseLocalizer
            );
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Amount,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnAmountCondition(condition, transactions, responseLocalizer);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Date,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnDateCondition(condition, transactions, responseLocalizer);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Account,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnAccountCondition(condition, transactions, responseLocalizer);
        }

        throw new BudgetBoardServiceException(
            responseLocalizer["AutomaticRuleUnsupportedFieldError", condition.Field]
        );
    }

    private static IEnumerable<Transaction> FilterOnMerchantCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.EqualsString,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                (t.MerchantName ?? "").Equals(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.NotEquals,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                !(t.MerchantName ?? "").Equals(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.Contains,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                (t.MerchantName ?? "").Contains(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.NotContains,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                !(t.MerchantName ?? "").Contains(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.StartsWith,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                (t.MerchantName ?? "").StartsWith(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.EndsWith,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t =>
                (t.MerchantName ?? "").EndsWith(
                    condition.Value,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.MatchesRegex,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            try
            {
                var regex = new System.Text.RegularExpressions.Regex(condition.Value);
                return transactions.Where(t => regex.IsMatch(t.MerchantName ?? ""));
            }
            catch (ArgumentException)
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutomaticRuleInvalidRegexError", condition.Value]
                );
            }
        }

        throw new BudgetBoardServiceException(
            responseLocalizer[
                "AutomaticRuleUnsupportedOperatorForMerchantError",
                condition.Operator
            ]
        );
    }

    private static IEnumerable<Transaction> FilterOnCategoryCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (!allCategories.Any(c => c.Value.Equals(condition.Value)))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleCategoryDoesNotExistError", condition.Value]
            );
        }

        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.Is,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            if (TransactionCategoriesHelpers.GetIsParentCategory(condition.Value, allCategories))
            {
                return transactions.Where(t => (t.Category ?? "").Equals(condition.Value));
            }
            else
            {
                return transactions.Where(t =>
                    (t.Subcategory ?? "").Equals(
                        condition.Value,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                );
            }
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.IsNot,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            if (TransactionCategoriesHelpers.GetIsParentCategory(condition.Value, allCategories))
            {
                return transactions.Where(t => !(t.Category ?? "").Equals(condition.Value));
            }
            else
            {
                return transactions.Where(t =>
                    !(t.Subcategory ?? "").Equals(
                        condition.Value,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                );
            }
        }

        throw new BudgetBoardServiceException(
            responseLocalizer[
                "AutomaticRuleUnsupportedOperatorForCategoryError",
                condition.Operator
            ]
        );
    }

    private static IEnumerable<Transaction> FilterOnAmountCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (!decimal.TryParse(condition.Value, out var conditionAmount))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidAmountError", condition.Value]
            );
        }

        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.EqualsString,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount == conditionAmount);
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.NotEquals,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount != conditionAmount);
        }
        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.GreaterThan,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount > conditionAmount);
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.LessThan,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount < conditionAmount);
        }

        throw new BudgetBoardServiceException(
            responseLocalizer["AutomaticRuleUnsupportedOperatorForAmountError", condition.Operator]
        );
    }

    private static IEnumerable<Transaction> FilterOnDateCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (!DateOnly.TryParse(condition.Value, out var conditionDate))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutomaticRuleInvalidDateError", condition.Value]
            );
        }

        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.On,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date == conditionDate);
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.Before,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date < conditionDate);
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.After,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date > conditionDate);
        }

        throw new BudgetBoardServiceException(
            responseLocalizer["AutomaticRuleUnsupportedOperatorForDateError", condition.Operator]
        );
    }

    private static IEnumerable<Transaction> FilterOnAccountCondition(
        IRuleParameterRequest condition,
        IEnumerable<Transaction> transactions,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        var accountIds = new HashSet<Guid>();
        foreach (
            var part in condition.Value.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
        )
        {
            if (!Guid.TryParse(part, out var parsed))
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutomaticRuleInvalidAccountIdError", part]
                );
            }
            accountIds.Add(parsed);
        }

        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.Is,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => accountIds.Contains(t.AccountID));
        }
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.IsNot,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => !accountIds.Contains(t.AccountID));
        }

        throw new BudgetBoardServiceException(
            responseLocalizer["AutomaticRuleUnsupportedOperatorForAccountError", condition.Operator]
        );
    }
}
