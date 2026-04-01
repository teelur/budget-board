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
    Task CreateTransactionCategoryAsync(Guid userGuid, ICategoryCreateRequest request);

    /// <summary>
    /// Retrieves transaction categories for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="categoryGuid">Optional. The unique identifier of a specific category to retrieve.</param>
    /// <returns>A collection of category details.</returns>
    Task<IReadOnlyList<ICategoryResponse>> ReadTransactionCategoriesAsync(
        Guid userGuid,
        Guid categoryGuid = default
    );

    /// <summary>
    /// Updates an existing transaction category.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The category update details.</param>
    Task UpdateTransactionCategoryAsync(Guid userGuid, ICategoryUpdateRequest request);

    /// <summary>
    /// Deletes a transaction category.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the category to delete.</param>
    Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid);
}
