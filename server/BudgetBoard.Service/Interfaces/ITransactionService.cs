using BudgetBoard.Service.Helpers;
using BudgetBoard.Database.Models;
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
    /// <param name="userData">The user data.</param>
    /// <param name="request">The transaction creation details.</param>
    /// <param name="allCategories">List of allcategories for the user.</param>
    /// <param name="autoCategorizer">The auto categorizer, if enabled and configured.</param>
    Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        IEnumerable<ICategory>? allCategories = null,
        AutomaticTransactionCategorizer? autoCategorizer = null);

    /// <summary>
    /// Creates a new transaction for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The transaction creation details.</param>
    /// <param name="allCategories">List of allcategories for the user.</param>
    /// <param name="autoCategorizer">The auto categorizer, if enabled and configured.</param>
    Task CreateTransactionAsync(
        Guid userGuid,
        ITransactionCreateRequest request,
        IEnumerable<ICategory>? allCategories = null,
        AutomaticTransactionCategorizer? autoCategorizer = null
    );

    /// <summary>
    /// Retrieves transactions for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="year">Optional. The year to filter transactions by.</param>
    /// <param name="month">Optional. The month to filter transactions by.</param>
    /// <param name="getHidden">If true, includes hidden transactions in the response.</param>
    /// <param name="guid">Optional. The unique identifier of a specific transaction to retrieve.</param>
    /// <returns>A collection of transaction details.</returns>
    Task<IReadOnlyList<ITransactionResponse>> ReadTransactionsAsync(
        Guid userGuid,
        int? year,
        int? month,
        bool getHidden,
        Guid guid = default
    );

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The transaction update details.</param>
    Task UpdateTransactionAsync(Guid userGuid, ITransactionUpdateRequest request);

    /// <summary>
    /// Deletes (soft deletes) a transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transactionID">The unique identifier of the transaction to delete.</param>
    Task DeleteTransactionAsync(Guid userGuid, Guid transactionID);

    /// <summary>
    /// Restores a previously deleted transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transactionID">The unique identifier of the transaction to restore.</param>
    Task RestoreTransactionAsync(Guid userGuid, Guid transactionID);

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
    Task ImportTransactionsAsync(
        Guid userGuid,
        ITransactionImportRequest request
    );
}
