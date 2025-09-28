using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBoard.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OidcController : ControllerBase
    {
        private readonly IExternalUserProvisioningService _provisioner;
        private readonly ILogger<OidcController> _logger;

        public OidcController(
            IExternalUserProvisioningService provisioner,
            ILogger<OidcController> logger
        )
        {
            _provisioner = provisioner ?? throw new ArgumentNullException(nameof(provisioner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("signin")]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var props = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(props, IdentityConstants.ExternalScheme);
        }

        // OIDC provider will redirect back to this endpoint; the OpenID Connect middleware
        // will populate the external principal on the ExternalScheme. This endpoint reads it,
        // delegates provisioning to the testable service, then redirects the user.
        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Principal == null || !result.Succeeded)
            {
                _logger.LogWarning(
                    "External authentication did not produce a principal or did not succeed."
                );
                return StatusCode(500, "External authentication failed.");
            }

            var provisioned = await _provisioner.ProvisionExternalUserAsync(
                result.Principal,
                HttpContext,
                IdentityConstants.ExternalScheme
            );

            // Clear the temporary external cookie (if any)
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (!provisioned)
            {
                _logger.LogWarning("Provisioning external user failed.");
                return StatusCode(500, "Failed to provision external user.");
            }

            // Prevent open redirect
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect("/");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(
                new
                {
                    User.Identity?.Name,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                }
            );
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Ok();
        }
    }
}
