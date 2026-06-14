using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

internal static class AccountTypeHelpers
{
    /// <summary>
    /// Combines built-in and custom account types (if enabled) for the specified user.
    /// </summary>
    /// <param name="userData">The user whose account types are to be retrieved.</param>
    /// <returns>A read-only list containing all applicable account types.</returns>
    internal static IReadOnlyList<IAccountTypeResponse> GetAllAccountTypes(ApplicationUser userData)
    {
        var allAccountTypes = new List<IAccountTypeResponse>();
        allAccountTypes.AddRange(
            userData.AccountTypes.Select(at => new AccountTypeResponse(at)).ToList()
        );

        if (userData.UserSettings?.DisableBuiltInAccountTypes != true)
        {
            allAccountTypes.AddRange(
                AccountTypeConstants
                    .DefaultAccountTypes.Select(at => new AccountTypeResponse(at))
                    .ToList()
            );
        }

        return allAccountTypes;
    }
}
