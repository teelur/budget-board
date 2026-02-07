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
}
