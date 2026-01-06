using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Manages LunchFlow accounts within the budgeting application.
/// </summary>
public interface ILunchFlowAccountService
{
    /// <summary>
    /// Creates a new LunchFlow account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request object containing details for the new LunchFlow account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateLunchFlowAccountAsync(Guid userGuid, ILunchFlowAccountCreateRequest request);

    /// <summary>
    /// Retrieves all LunchFlow accounts associated with the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of LunchFlow account responses.</returns>
    Task<IReadOnlyList<ILunchFlowAccountResponse>> ReadLunchFlowAccountsAsync(Guid userGuid);

    /// <summary>
    /// Updates an existing LunchFlow account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request object containing updated details for the LunchFlow account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateLunchFlowAccountAsync(Guid userGuid, ILunchFlowAccountUpdateRequest request);

    /// <summary>
    /// Deletes a LunchFlow account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountGuid">The unique identifier of the LunchFlow account to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteLunchFlowAccountAsync(Guid userGuid, Guid accountGuid);

    /// <summary>
    /// Updates the linked account for a specific LunchFlow account.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="lunchFlowAccountGuid">The unique identifier of the LunchFlow account.</param>
    /// <param name="linkedAccountGuid">The unique identifier of the linked account, or null to unlink.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateLinkedAccountAsync(
        Guid userGuid,
        Guid lunchFlowAccountGuid,
        Guid? linkedAccountGuid
    );
}
