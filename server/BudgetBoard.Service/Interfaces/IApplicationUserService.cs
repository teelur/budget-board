using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing application user data.
/// </summary>
public interface IApplicationUserService
{
    /// <summary>
    /// Retrieves the details of a specific application user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userManager">The user manager instance to retrieve login information.</param>
    /// <returns>The application user details.</returns>
    Task<IApplicationUserResponse> ReadApplicationUserAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    );

    /// <summary>
    /// Updates the details of an existing application user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The user update details.</param>
    Task UpdateApplicationUserAsync(Guid userGuid, IApplicationUserUpdateRequest request);
}
