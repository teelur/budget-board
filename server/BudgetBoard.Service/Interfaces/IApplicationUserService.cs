using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetBoard.Service.Interfaces;

public interface IApplicationUserService
{
    Task<IApplicationUserResponse> ReadApplicationUserAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    );
    Task UpdateApplicationUserAsync(Guid userGuid, IApplicationUserUpdateRequest user);
}
