using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Overrides;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

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
        ILogger<OidcController> logger,
        IStringLocalizer<ApiLogStrings> logLocalizer,
        IStringLocalizer<ApiResponseStrings> responseLocalizer
    ) : ControllerBase
    {
        private readonly IExternalUserProvisioningService _provisioner =
            provisioner ?? throw new ArgumentNullException(nameof(provisioner));
        private readonly IOidcTokenService _tokenService =
            tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly ILogger<OidcController> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;
        private readonly IStringLocalizer<ApiResponseStrings> _responseLocalizer =
            responseLocalizer;

        // Frontend calls this endpoint after receiving authorization code from OIDC provider
        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] OidcCallbackRequest request)
        {
            _logger.LogInformation("{LogMessage}", _logLocalizer["OidcCallbackStartedLog"]);

            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning("{LogMessage}", _logLocalizer["OidcNoAuthCodeLog"]);
                return BadRequest(_responseLocalizer["AuthCodeRequired"].Value);
            }

            try
            {
                // Exchange authorization code for user claims
                var principal = await _tokenService.ExchangeCodeForUserAsync(
                    request.Code,
                    request.RedirectUri
                );
                if (principal == null)
                {
                    _logger.LogError("{LogMessage}", _logLocalizer["OidcExchangeFailedLog"]);
                    return StatusCode(500, _responseLocalizer["AuthFailed"].Value);
                }

                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer["OidcExchangeSucceededLog", principal.Claims?.Count() ?? 0]
                );

                // Provision the user in our system
                var provisioned = await _provisioner.ProvisionExternalUserAsync(
                    principal,
                    HttpContext,
                    IdentityApiEndpointRouteBuilderConstants.OidcLoginProvider
                );

                if (!provisioned)
                {
                    _logger.LogWarning("{LogMessage}", _logLocalizer["OidcProvisioningFailedLog"]);
                    return StatusCode(500, _responseLocalizer["LoginFailed"].Value);
                }

                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer["OidcProvisioningSucceededLog"]
                );

                // Return success response for frontend to handle
                return Ok(new OidcCallbackResponse { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{LogMessage}", _logLocalizer["OidcCallbackErrorLog"]);
                return StatusCode(500, _responseLocalizer["AuthFailed"].Value);
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
