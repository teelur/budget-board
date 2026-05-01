using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

internal static class AccountTypeHelpers
{
    /// <summary>
    /// Combines built-in and custom account types for the specified user.
    /// </summary>
    /// <param name="userData">The user whose account types are to be retrieved.</param>
    /// <returns>A read-only list containing all applicable account types.</returns>
    internal static IReadOnlyList<IAccountType> GetAllAccountTypes(ApplicationUser userData)
    {
        var customAccountTypes = userData.AccountTypes.Select(at => new AccountTypeBase()
        {
            Value = at.Value,
            Parent = at.Parent,
        });

        var allAccountTypes = new List<IAccountType>();
        allAccountTypes.AddRange(AccountTypeConstants.DefaultAccountTypes);
        allAccountTypes.AddRange(customAccountTypes);
        return allAccountTypes;
    }
}
