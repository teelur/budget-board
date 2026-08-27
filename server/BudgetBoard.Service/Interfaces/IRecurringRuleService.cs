using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing recurring transaction rules.
/// </summary>
public interface IRecurringRuleService
{
    /// <summary>
    /// Retrieves the recurring rules for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A collection of recurring rule details.</returns>
    Task<IReadOnlyList<IRecurringRuleResponse>> ReadRecurringRulesAsync(Guid userGuid);

    /// <summary>
    /// Creates a recurring rule for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The recurring rule creation details.</param>
    /// <param name="transactionID">Optional. The unique identifier of a transaction to associate with the rule.</param>
    /// <returns>The created recurring rule details.</returns>
    Task<IRecurringRuleResponse> CreateRecurringRuleAsync(
        Guid userGuid,
        IRecurringRuleRequest request,
        Guid? transactionID = null
    );

    /// <summary>
    /// Updates an existing recurring rule.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The recurring rule update details.</param>
    /// <returns>The updated recurring rule details.</returns>
    Task<IRecurringRuleResponse> UpdateRecurringRuleAsync(
        Guid userGuid,
        IRecurringRuleUpdateRequest request
    );

    /// <summary>
    /// Deletes a recurring rule.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="recurringRuleID">The unique identifier of the recurring rule to delete.</param>
    Task DeleteRecurringRuleAsync(Guid userGuid, Guid recurringRuleID);

    /// <summary>
    /// Retrieves forecast occurrences for the specified month.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="month">The month for which to retrieve forecast occurrences.</param>
    /// <returns>A collection of forecast occurrences.</returns>
    Task<IReadOnlyList<RecurringForecastOccurrenceResponse>> ReadForecastAsync(
        Guid userGuid,
        DateOnly month
    );

    /// <summary>
    /// Attempts to associate a transaction with its unambiguous matching recurring rule.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transaction">The transaction to match.</param>
    Task MatchTransactionAsync(Guid userGuid, Transaction transaction);

    /// <summary>
    /// Assigns a transaction to a recurring rule.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="recurringRuleID">The unique identifier of the recurring rule.</param>
    /// <param name="transactionID">The unique identifier of the transaction.</param>
    Task AssignTransactionAsync(Guid userGuid, Guid recurringRuleID, Guid transactionID);

    /// <summary>
    /// Removes the recurring rule assignment from a transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transactionID">The unique identifier of the transaction.</param>
    Task UnassignTransactionAsync(Guid userGuid, Guid transactionID);
}
