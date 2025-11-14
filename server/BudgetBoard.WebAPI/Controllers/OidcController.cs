using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BudgetBoard.WebAPI.Controllers
{
    /// <summary>
    /// Attribute to disable controller when OIDC is not enabled
    /// </summary>
    public class RequireOidcEnabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var configuration =
                context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var oidcEnabled = configuration.GetValue<bool>("OIDC_ENABLED");

            if (!oidcEnabled)
            {
                context.Result = new NotFoundResult();
            }

            base.OnActionExecuting(context);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    [RequireOidcEnabled]
    public class OidcController(
        IExternalUserProvisioningService provisioner,
        IOidcTokenService tokenService,
        ILogger<OidcController> logger
    ) : ControllerBase
    {
        private readonly IExternalUserProvisioningService _provisioner =
            provisioner ?? throw new ArgumentNullException(nameof(provisioner));
        private readonly IOidcTokenService _tokenService =
            tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly ILogger<OidcController> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));

        // Frontend calls this endpoint after receiving authorization code from OIDC provider
        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] OidcCallbackRequest request)
        {
            _logger.LogInformation("OIDC callback started.");

            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning("No authorization code provided");
                return BadRequest("Authorization code is required.");
            }

            try
            {
                // Exchange authorization code for user claims
                var principal = await _tokenService.ExchangeCodeForUserAsync(
                    request.Code,
                    request.RedirectUri ?? string.Empty
                );
                if (principal == null)
                {
                    _logger.LogError("Failed to exchange code for user principal");
                    return StatusCode(500, "Authentication failed.");
                }

                _logger.LogInformation(
                    "Token exchange succeeded. Claims count: {ClaimCount}",
                    principal.Claims?.Count() ?? 0
                );

                // Provision the user in our system
                var provisioned = await _provisioner.ProvisionExternalUserAsync(
                    principal,
                    HttpContext,
                    "oidc"
                );

                if (!provisioned)
                {
                    _logger.LogWarning("User provisioning failed");
                    return StatusCode(500, "Unable to complete login.");
                }

                _logger.LogInformation("User provisioning succeeded.");

                // Return success response for frontend to handle
                return Ok(new OidcCallbackResponse { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OIDC callback processing");
                return StatusCode(500, "Authentication failed.");
            }
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
    }
}
