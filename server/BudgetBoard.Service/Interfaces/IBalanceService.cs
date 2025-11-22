using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing account balances.
/// </summary>
public interface IBalanceService
{
    /// <summary>
    /// Creates a new balance entry for a specific account.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="balance">The balance creation details.</param>
    Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest balance);

    /// <summary>
    /// Retrieves balance history for a specific account.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <returns>A collection of balance entries.</returns>
    Task<IReadOnlyList<IBalanceResponse>> ReadBalancesAsync(Guid userGuid, Guid accountId);

    /// <summary>
    /// Updates an existing balance entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="updatedBalance">The balance update details.</param>
    Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest updatedBalance);

    /// <summary>
    /// Deletes (soft deletes) a balance entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the balance to delete.</param>
    Task DeleteBalanceAsync(Guid userGuid, Guid guid);

    /// <summary>
    /// Restores a previously deleted balance entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the balance to restore.</param>
    Task RestoreBalanceAsync(Guid userGuid, Guid guid);
}
