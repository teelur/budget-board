using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing transaction categories.
/// </summary>
public interface ITransactionCategoryService
{
    /// <summary>
    /// Creates a new transaction category for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The category creation details.</param>
    Task CreateTransactionCategoryAsync(Guid userGuid, ITransactionCategoryCreateRequest request);

    /// <summary>
    /// Retrieves transaction categories for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A collection of category details.</returns>
    Task<IReadOnlyList<ITransactionCategoryResponse>> ReadTransactionCategoriesAsync(Guid userGuid);

    /// <summary>
    /// Updates an existing transaction category.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The category update details.</param>
    Task UpdateTransactionCategoryAsync(Guid userGuid, ITransactionCategoryUpdateRequest request);

    /// <summary>
    /// Clears built-in category references from the user's transactions and custom categories.
    /// This only changes the current EF unit of work; the caller owns SaveChangesAsync.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    Task ClearBuiltInTransactionCategoryReferencesAsync(Guid userGuid);

    /// <summary>
    /// Deletes a transaction category.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the category to delete.</param>
    Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid);
}
