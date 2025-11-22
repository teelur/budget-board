using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Services.Interfaces;

/// <summary>
/// Service for managing user accounts, including creation, retrieval, updates, and deletion.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The account creation details.</param>
    Task CreateAccountAsync(Guid userGuid, IAccountCreateRequest request);

    /// <summary>
    /// Retrieves accounts for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountGuid">Optional. The unique identifier of a specific account to retrieve.</param>
    /// <returns>A collection of account details.</returns>
    Task<IReadOnlyList<IAccountResponse>> ReadAccountsAsync(
        Guid userGuid,
        Guid accountGuid = default
    );

    /// <summary>
    /// Updates an existing account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="editedAccount">The account update details.</param>
    Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest request);

    /// <summary>
    /// Deletes (soft deletes) an account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountGuid">The unique identifier of the account to delete.</param>
    /// <param name="deleteTransactions">If true, also deletes transactions for the account.</param>
    Task DeleteAccountAsync(Guid userGuid, Guid accountGuid, bool deleteTransactions = false);

    /// <summary>
    /// Restores a previously deleted account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountGuid">The unique identifier of the account to restore.</param>
    /// <param name="restoreTransactions">If true, also restores transactions for the account.</param>
    Task RestoreAccountAsync(Guid userGuid, Guid accountGuid, bool restoreTransactions = false);

    /// <summary>
    /// Updates the order of accounts for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="orderedAccounts">A collection of account index requests defining the new order.</param>
    Task OrderAccountsAsync(Guid userGuid, IEnumerable<IAccountIndexRequest> orderedAccounts);
}
