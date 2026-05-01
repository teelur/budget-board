using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing account types.
/// </summary>
public interface IAccountTypeService
{
    /// <summary>
    /// Creates a new account type for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The account type creation details.</param>
    Task CreateAccountTypeAsync(Guid userGuid, IAccountTypeCreateRequest request);

    /// <summary>
    /// Retrieves account types for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="accountTypeGuid">Optional. The unique identifier of a specific account type to retrieve.</param>
    /// <returns>A collection of account type details.</returns>
    Task<IReadOnlyList<IAccountTypeResponse>> ReadAccountTypesAsync(
        Guid userGuid,
        Guid accountTypeGuid = default
    );

    /// <summary>
    /// Updates an existing account type.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The account type update details.</param>
    Task UpdateAccountTypeAsync(Guid userGuid, IAccountTypeUpdateRequest request);

    /// <summary>
    /// Deletes an account type.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the account type to delete.</param>
    Task DeleteAccountTypeAsync(Guid userGuid, Guid guid);
}
