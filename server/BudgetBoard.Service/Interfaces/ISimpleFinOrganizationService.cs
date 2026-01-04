using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing SimpleFIN organizations, including creation, retrieval, updates, and deletion.
/// </summary>
public interface ISimpleFinOrganizationService
{
    /// <summary>
    /// Creates a SimpleFIN organization for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request containing details for creating a SimpleFIN organization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateSimpleFinOrganizationAsync(
        Guid userGuid,
        ISimpleFinOrganizationCreateRequest request
    );

    /// <summary>
    /// Reads all SimpleFIN organizations associated with the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation, containing a read-only list of SimpleFIN organization responses.</returns>
    Task<IReadOnlyList<ISimpleFinOrganizationResponse>> ReadSimpleFinOrganizationsAsync(
        Guid userGuid
    );

    /// <summary>
    /// Updates a SimpleFIN organization for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The request containing details for updating a SimpleFIN organization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateSimpleFinOrganizationAsync(
        Guid userGuid,
        ISimpleFinOrganizationUpdateRequest request
    );

    /// <summary>
    /// Deletes a SimpleFIN organization for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="organizationGuid">The unique identifier of the SimpleFIN organization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSimpleFinOrganizationAsync(Guid userGuid, Guid organizationGuid);
}
