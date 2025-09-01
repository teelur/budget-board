using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

public static class AutomaticRuleHelpers
{
    public static IEnumerable<Transaction> FilterOnCondition(
        IRuleParameterResponse condition,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories
    )
    {
        if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Merchant,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnMerchantCondition(condition, transactions);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Category,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnCategoryCondition(condition, transactions, allCategories);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Amount,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnAmountCondition(condition, transactions);
        }
        else if (
            condition.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Date,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return FilterOnDateCondition(condition, transactions);
        }

        throw new BudgetBoardServiceException(
            $"Unsupported field '{condition.Field}' in rule condition."
        );
    }

    public static async Task<int> ApplyActionToTransactions(
        IRuleParameterResponse action,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Merchant,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplyActionForMerchant(action, transactions, transactionService, userGuid);
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Category,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplyActionForCategory(
                action,
                transactions,
                allCategories,
                transactionService,
                userGuid
            );
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Amount,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplyActionForAmount(action, transactions, transactionService, userGuid);
        }
        else if (
            action.Field.Equals(
                AutomaticRuleConstants.TransactionFields.Date,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return await ApplyActionForDate(action, transactions, transactionService, userGuid);
        }
        else
        {
            throw new BudgetBoardServiceException(
                $"Unsupported field '{action.Field}' in rule action."
            );
        }
    }

    private static IEnumerable<Transaction> FilterOnMerchantCondition(
        IRuleParameterResponse condition,
        IEnumerable<Transaction> transactions
    )
    {
        // Equals
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
        // Not equals
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
        // Contains
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
        // Not contains
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
        // Starts with
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
        // Ends with
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
        // Matches regex
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
                    $"The regex pattern '{condition.Value}' is not valid."
                );
            }
        }

        throw new BudgetBoardServiceException(
            $"Unsupported operator '{condition.Operator}' for Merchant field."
        );
    }

    private static IEnumerable<Transaction> FilterOnCategoryCondition(
        IRuleParameterResponse condition,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories
    )
    {
        if (
            condition.Value != string.Empty
            && !allCategories.Any(c => c.Value.Equals(condition.Value))
        )
        {
            throw new BudgetBoardServiceException(
                $"The category '{condition.Value}' does not exist."
            );
        }

        // Is
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
        // Is not
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
            $"Unsupported operator '{condition.Operator}' for Category field."
        );
    }

    private static IEnumerable<Transaction> FilterOnAmountCondition(
        IRuleParameterResponse condition,
        IEnumerable<Transaction> transactions
    )
    {
        if (!decimal.TryParse(condition.Value, out var conditionAmount))
        {
            throw new BudgetBoardServiceException(
                $"The amount '{condition.Value}' is not a valid decimal number."
            );
        }

        // Equals
        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.EqualsString,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount == conditionAmount);
        }
        // Not equals
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.NotEquals,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount != conditionAmount);
        }
        // Greater than
        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.GreaterThan,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Amount > conditionAmount);
        }
        // Less than
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
            $"Unsupported operator '{condition.Operator}' for Amount field."
        );
    }

    private static IEnumerable<Transaction> FilterOnDateCondition(
        IRuleParameterResponse condition,
        IEnumerable<Transaction> transactions
    )
    {
        if (!DateTime.TryParse(condition.Value, out var conditionDate))
        {
            throw new BudgetBoardServiceException(
                $"The date '{condition.Value}' is not a valid date."
            );
        }

        // On
        if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.On,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date.Date == conditionDate.Date);
        }
        // Before
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.Before,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date.Date < conditionDate.Date);
        }
        // After
        else if (
            condition.Operator.Equals(
                AutomaticRuleConstants.ConditionalOperators.After,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return transactions.Where(t => t.Date.Date > conditionDate.Date);
        }

        throw new BudgetBoardServiceException(
            $"Unsupported operator '{condition.Operator}' for Date field."
        );
    }

    private static async Task<int> ApplyActionForMerchant(
        IRuleParameterResponse action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Set,
                StringComparison.CurrentCultureIgnoreCase
            )
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

        throw new BudgetBoardServiceException(
            $"Unsupported operator '{action.Operator}' in rule action."
        );
    }

    private static async Task<int> ApplyActionForCategory(
        IRuleParameterResponse action,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Set,
                StringComparison.CurrentCultureIgnoreCase
            )
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
                        $"The category '{action.Value}' does not exist."
                    );
                }
            }

            int updatedTransactions = 0;
            foreach (var transaction in transactions)
            {
                transaction.Category = TransactionCategoriesHelpers.GetParentCategory(
                    newCategory,
                    allCategories
                );
                transaction.Subcategory = TransactionCategoriesHelpers.GetIsParentCategory(
                    newCategory,
                    allCategories
                )
                    ? ""
                    : action.Value;

                await transactionService.UpdateTransactionAsync(
                    userGuid,
                    new TransactionUpdateRequest(transaction)
                    {
                        Category = TransactionCategoriesHelpers.GetParentCategory(
                            newCategory,
                            allCategories
                        ),
                        Subcategory = TransactionCategoriesHelpers.GetIsParentCategory(
                            newCategory,
                            allCategories
                        )
                            ? ""
                            : action.Value,
                    }
                );

                updatedTransactions++;
            }
            return updatedTransactions;
        }

        throw new BudgetBoardServiceException(
            $"Unsupported operator '{action.Operator}' in rule action."
        );
    }

    private static async Task<int> ApplyActionForAmount(
        IRuleParameterResponse action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Set,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            if (!decimal.TryParse(action.Value, out var newAmount))
            {
                throw new BudgetBoardServiceException(
                    $"The amount '{action.Value}' is not a valid decimal number."
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

        throw new BudgetBoardServiceException(
            $"Unsupported operator '{action.Operator}' in rule action."
        );
    }

    private static async Task<int> ApplyActionForDate(
        IRuleParameterResponse action,
        IEnumerable<Transaction> transactions,
        ITransactionService transactionService,
        Guid userGuid
    )
    {
        if (
            action.Operator.Equals(
                AutomaticRuleConstants.ActionOperators.Set,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            if (!DateTime.TryParse(action.Value, out var newDate))
            {
                throw new BudgetBoardServiceException(
                    $"The date '{action.Value}' is not a valid date."
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
            $"Unsupported operator '{action.Operator}' in rule action."
        );
    }
}
