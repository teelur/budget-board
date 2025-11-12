using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            _logger.LogInformation(
                "OIDC callback started. Code: {Code}, ReturnUrl: {ReturnUrl}",
                string.IsNullOrEmpty(request.Code) ? "null" : "provided",
                request.ReturnUrl
            );

            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning("No authorization code provided");
                return BadRequest("Authorization code is required");
            }

            try
            {
                // Exchange authorization code for user claims
                var principal = await _tokenService.ExchangeCodeForUserAsync(request.Code);
                if (principal == null)
                {
                    _logger.LogError("Failed to exchange code for user principal");
                    return StatusCode(500, "Token exchange failed");
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
                    return StatusCode(500, "Failed to provision external user");
                }

                _logger.LogInformation(
                    "User provisioning succeeded. ReturnUrl: {ReturnUrl}",
                    request.ReturnUrl
                );

                // Return success response for frontend to handle
                return Ok(
                    new OidcCallbackResponse
                    {
                        Success = true,
                        ReturnUrl = request.ReturnUrl ?? "/",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OIDC callback processing");
                return StatusCode(500, "Authentication failed");
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
