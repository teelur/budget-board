using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Overrides;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

public class ApplicationUserConstants
{
    public const string UserType =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
}

[Route("api/[controller]")]
[ApiController]
public class ApplicationUserController(
    ILogger<ApplicationUserController> logger,
    UserManager<ApplicationUser> userManager,
    UserDataContext context,
    IApplicationUserService applicationUserService,
    ISyncService simpleFinService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    private readonly ILogger<ApplicationUserController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly UserDataContext _userDataContext = context;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly ISyncService _simpleFinService = simpleFinService;
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;
    private readonly IStringLocalizer<ApiResponseStrings> _responseLocalizer = responseLocalizer;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await _applicationUserService.ReadApplicationUserAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    _userManager
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] ApplicationUserUpdateRequest newUser)
    {
        try
        {
            await _applicationUserService.UpdateApplicationUserAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                newUser
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult IsSignedIn() => Ok(HttpContext.User?.Identity?.IsAuthenticated ?? false);

    [HttpDelete]
    [Route("[action]")]
    [Authorize]
    public async Task<IActionResult> DisconnectOidcLogin()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning(_logLocalizer["UserIdNotFoundLog"]);
                return Unauthorized(_responseLocalizer["UserNotAuthenticated"].Value);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(_logLocalizer["UserNotFoundLog"], userId);
                return NotFound(_responseLocalizer["UserNotFound"].Value);
            }

            // Get all external logins for the user
            var logins = await _userManager.GetLoginsAsync(user);
            var oidcLogin = logins.FirstOrDefault(l =>
                l.LoginProvider == IdentityApiEndpointRouteBuilderConstants.OidcLoginProvider
            );

            if (oidcLogin == null)
            {
                _logger.LogWarning(_logLocalizer["NoOidcLoginFoundLog"], userId);
                return BadRequest(_responseLocalizer["NoOidcLoginFound"].Value);
            }

            // Check if user has a password set (local auth) before removing OIDC
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var remainingLogins = logins.Count(l =>
                l.LoginProvider != IdentityApiEndpointRouteBuilderConstants.OidcLoginProvider
            );
            if (!hasPassword && remainingLogins == 0)
            {
                _logger.LogWarning(_logLocalizer["RemoveOidcNoPasswordLog"], userId);
                return BadRequest(_responseLocalizer["RemoveOidcNoPassword"].Value);
            }

            // Remove the OIDC login
            var result = await _userManager.RemoveLoginAsync(
                user,
                oidcLogin.LoginProvider,
                oidcLogin.ProviderKey
            );

            if (!result.Succeeded)
            {
                _logger.LogError(
                    _logLocalizer["RemoveOidcFailedLog"],
                    userId,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
                return StatusCode(500, _responseLocalizer["RemoveOidcFailed"].Value);
            }

            _logger.LogInformation(_logLocalizer["RemoveOidcSuccessLog"], userId);
            return Ok(new { message = _responseLocalizer["RemoveOidcSuccess"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }
}
