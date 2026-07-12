using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing automatic transaction categorizer ML model training.
/// </summary>
public interface IAutomaticTransactionCategorizerService
{
    /// <summary>
    /// Trains a model for the given user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The training request details.</param>
    Task TrainCategorizerAsync(Guid userGuid, ITrainAutoCategorizerRequest request);

    /// <summary>
    /// Automatically categorizes a transaction for the given user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transaction">The transaction to categorize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AutoCategorizeTransactionAsync(Guid userGuid, Transaction transaction);
}
