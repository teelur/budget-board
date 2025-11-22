using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing financial institutions.
/// </summary>
public interface IInstitutionService
{
    /// <summary>
    /// Creates a new financial institution for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The institution creation details.</param>
    Task CreateInstitutionAsync(Guid userGuid, IInstitutionCreateRequest request);

    /// <summary>
    /// Retrieves financial institutions for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="guid">Optional. The unique identifier of a specific institution to retrieve.</param>
    /// <returns>A collection of institution details.</returns>
    Task<IReadOnlyList<IInstitutionResponse>> ReadInstitutionsAsync(
        Guid userGuid,
        Guid guid = default
    );

    /// <summary>
    /// Updates an existing financial institution.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The institution update details.</param>
    Task UpdateInstitutionAsync(Guid userGuid, IInstitutionUpdateRequest request);

    /// <summary>
    /// Deletes (soft deletes) a financial institution.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="id">The unique identifier of the institution to delete.</param>
    /// <param name="deleteTransactions">If true, also deletes associated transactions.</param>
    Task DeleteInstitutionAsync(Guid userGuid, Guid id, bool deleteTransactions);

    /// <summary>
    /// Updates the order of institutions for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="orderedInstitutions">A collection of institution index requests defining the new order.</param>
    Task OrderInstitutionsAsync(
        Guid userGuid,
        IEnumerable<IInstitutionIndexRequest> orderedInstitutions
    );
}
