using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing SimpleFIN accounts, including creation, retrieval, updates, and deletion.
/// </summary>
public interface ISimpleFinAccountService
{
    /// <summary>
    /// Creates a SimpleFIN account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request containing details for creating a SimpleFIN account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateSimpleFinAccountAsync(Guid userGuid, ISimpleFinAccountCreateRequest request);

    /// <summary>
    /// Reads all SimpleFIN accounts associated with the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation, containing a read-only list of SimpleFIN account responses.</returns>
    Task<IReadOnlyList<ISimpleFinAccountResponse>> ReadSimpleFinAccountsAsync(Guid userGuid);

    /// <summary>
    /// Updates a SimpleFIN account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request containing details for updating a SimpleFIN account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateSimpleFinAccountAsync(Guid userGuid, ISimpleFinAccountUpdateRequest request);

    /// <summary>
    /// Deletes a SimpleFIN account for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountGuid">The unique identifier of the SimpleFIN account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSimpleFinAccountAsync(Guid userGuid, Guid accountGuid);

    /// <summary>
    /// Updates the linked account for a SimpleFIN account.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="simpleFinAccountGuid">The unique identifier of the SimpleFIN account.</param>
    /// <param name="linkedAccountGuid">The unique identifier of the linked account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateLinkedAccountAsync(
        Guid userGuid,
        Guid simpleFinAccountGuid,
        Guid? linkedAccountGuid
    );
}
