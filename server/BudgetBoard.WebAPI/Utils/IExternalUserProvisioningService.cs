using System.Security.Claims;

namespace BudgetBoard.Utils
{
    public sealed record ExternalUserProvisioningResult(
        bool Succeeded,
        bool RequiresExplicitLinking
    )
    {
        public static ExternalUserProvisioningResult Success() => new(true, false);

        public static ExternalUserProvisioningResult Failed() => new(false, false);

        public static ExternalUserProvisioningResult ExplicitLinkingRequired() => new(false, true);
    }

    public interface IExternalUserProvisioningService
    {
        /// <summary>
        /// Ensures a local ApplicationUser exists for the external identity, associates the external login,
        /// and signs the user into the application cookie.
        /// Returns a result describing success/failure and whether explicit account linking is required.
        /// </summary>
        Task<ExternalUserProvisioningResult> ProvisionExternalUserAsync(
            ClaimsPrincipal principal,
            HttpContext httpContext,
            string schemeName,
            bool isPersistent = false
        );
    }
}
