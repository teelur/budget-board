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
    /// Retrieves all account types for the specified user. This includes both built-in (if configured) and custom account types.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">Optional. When provided, returns only the account type with this ID.</param>
    /// <returns>A collection of account type details.</returns>
    Task<IReadOnlyList<IAccountTypeResponse>> ReadAccountTypesAsync(Guid userGuid);

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
