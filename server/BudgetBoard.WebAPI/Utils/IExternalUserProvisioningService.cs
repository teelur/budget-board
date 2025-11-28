using System.Security.Claims;

namespace BudgetBoard.Utils
{
    public interface IExternalUserProvisioningService
    {
        /// <summary>
        /// Ensures a local ApplicationUser exists for the external identity, associates the external login,
        /// and signs the user into the application cookie.
        /// Returns true on success, false on failure.
        /// </summary>
        Task<bool> ProvisionExternalUserAsync(
            ClaimsPrincipal principal,
            HttpContext httpContext,
            string schemeName
        );
    }
}
