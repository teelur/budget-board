using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing financial goals.
/// </summary>
public interface IGoalService
{
    /// <summary>
    /// Creates a new financial goal for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The goal creation details.</param>
    Task CreateGoalAsync(Guid userGuid, IGoalCreateRequest request);

    /// <summary>
    /// Retrieves financial goals for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="includeInterest">If true, includes interest calculations in the response.</param>
    /// <returns>A collection of goal details.</returns>
    Task<IReadOnlyList<IGoalResponse>> ReadGoalsAsync(Guid userGuid, bool includeInterest);

    /// <summary>
    /// Updates an existing financial goal.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The goal update details.</param>
    Task UpdateGoalAsync(Guid userGuid, IGoalUpdateRequest request);

    /// <summary>
    /// Deletes a financial goal.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">The unique identifier of the goal to delete.</param>
    Task DeleteGoalAsync(Guid userGuid, Guid guid);

    /// <summary>
    /// Marks a financial goal as complete.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="goalID">The unique identifier of the goal to complete.</param>
    /// <param name="completedDate">The date the goal was completed.</param>
    Task CompleteGoalAsync(Guid userGuid, Guid goalID, DateTime completedDate);

    /// <summary>
    /// Marks all eligible financial goals as complete.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompleteGoalsAsync(Guid userGuid);
}
