using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing financial transactions.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The transaction creation details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateTransactionAsync(Guid userGuid, ITransactionCreateRequest request);

    /// <summary>
    /// Creates a new transaction for the specified user.
    /// </summary>
    /// <param name="userData">The user data.</param>
    /// <param name="request">The transaction creation details.</param>
    /// <param name="deferSave">Optional. If true, defers saving changes to the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        bool deferSave = false
    );

    /// <summary>
    /// Retrieves transactions for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="year">Optional. The year to filter transactions by.</param>
    /// <param name="month">Optional. The month to filter transactions by.</param>
    /// <param name="includeHidden">If true, includes hidden transactions in the response.</param>
    /// <param name="includeDeleted">If true, includes deleted transactions in the response.</param>
    /// <returns>A collection of transaction details sorted by date in descending order.</returns>
    Task<IReadOnlyList<ITransactionResponse>> ReadTransactionsAsync(
        Guid userGuid,
        int? year,
        int? month,
        bool includeHidden,
        bool includeDeleted
    );

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="requests">The list of transaction update details.</param>
    Task UpdateTransactionsAsync(Guid userGuid, IEnumerable<ITransactionUpdateRequest> requests);

    /// <summary>
    /// Deletes (soft deletes) transactions.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transactionIDs">The collection of unique identifiers of the transactions to delete.</param>
    /// <param name="deferSave">Optional. If true, defers saving changes to the database.</param>
    Task DeleteTransactionsAsync(
        Guid userGuid,
        IEnumerable<Guid> transactionIDs,
        bool deferSave = false
    );

    /// <summary>
    /// Restores previously deleted transactions.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transactionIDs">The collection of unique identifiers of the transactions to restore.</param>
    /// <param name="deferSave">Optional. If true, defers saving changes to the database.</param>
    Task RestoreTransactionsAsync(
        Guid userGuid,
        IEnumerable<Guid> transactionIDs,
        bool deferSave = false
    );

    /// <summary>
    /// Splits a transaction into two parts.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The transaction split details.</param>
    Task SplitTransactionAsync(Guid userGuid, ITransactionSplitRequest request);

    /// <summary>
    /// Imports a batch of transactions.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The transaction import details.</param>
    Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest request);
}
