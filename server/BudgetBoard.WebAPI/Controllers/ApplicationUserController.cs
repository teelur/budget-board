using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    ISimpleFinService simpleFinService
) : ControllerBase
{
    private readonly ILogger<ApplicationUserController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly UserDataContext _userDataContext = context;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly ISimpleFinService _simpleFinService = simpleFinService;

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
            _logger.LogError(ex, "An unexpected error occurred.");
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
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
            _logger.LogError(ex, "An unexpected error occurred.");
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
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
                _logger.LogWarning("User ID not found in the current context.");
                return Unauthorized("User is not authenticated.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for ID {UserId}.", userId);
                return NotFound("User not found.");
            }

            // Get all external logins for the user
            var logins = await _userManager.GetLoginsAsync(user);
            var oidcLogin = logins.FirstOrDefault(l => l.LoginProvider == "oidc");

            if (oidcLogin == null)
            {
                _logger.LogWarning("No OIDC login found for user {UserId}.", userId);
                return BadRequest("No OIDC login found for this user.");
            }

            // Check if user has a password set (local auth) before removing OIDC
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var remainingLogins = logins.Count(l => l.LoginProvider != "oidc");
            if (!hasPassword && remainingLogins == 0)
            {
                _logger.LogWarning(
                    "Attempt to remove OIDC login for user {UserId} without a local password set.",
                    userId
                );
                return BadRequest(
                    "Cannot remove OIDC login. User must have a local password set first to maintain account access."
                );
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
                    "Failed to remove OIDC login for user {UserId}: {Errors}",
                    userId,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
                return StatusCode(500, "Failed to remove OIDC login.");
            }

            _logger.LogInformation("Successfully removed OIDC login for user {UserId}", userId);
            return Ok(new { message = "OIDC login disconnected successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while disconnecting OIDC login.");
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }
}
