using System.Security.Claims;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationUserController(
    ILogger<ApplicationUserController> logger,
    UserManager<ApplicationUser> userManager,
    IApplicationUserService applicationUserService,
    IOidcTokenService oidcTokenService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<ApplicationUserController>(logger, logLocalizer, responseLocalizer)
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(
                await applicationUserService.ReadApplicationUserAsync(parsedUserId, userManager)
            );
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] ApplicationUserUpdateRequest newUser)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await applicationUserService.UpdateApplicationUserAsync(parsedUserId, newUser);
            return Ok();
        });
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult IsSignedIn() => Ok(HttpContext.User?.Identity?.IsAuthenticated ?? false);

    [HttpDelete]
    [Route("[action]")]
    [Authorize]
    public async Task<IActionResult> DisconnectOidcLogin()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await applicationUserService.DisconnectOidcLoginAsync(parsedUserId, userManager);
            return Ok();
        });
    }

    [HttpPost]
    [Route("[action]")]
    [Authorize]
    public async Task<IActionResult> ConnectOidcLogin([FromBody] OidcCallbackRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(responseLocalizer["AuthCodeRequired"].Value);
            }

            var principal = await oidcTokenService.ExchangeCodeForUserAsync(
                request.Code,
                request.RedirectUri
            );
            if (principal == null)
            {
                return StatusCode(500, responseLocalizer["AuthFailed"].Value);
            }

            var oidcEmail =
                principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
            if (string.IsNullOrWhiteSpace(oidcEmail))
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer["OidcConnectEmailClaimMissingLog", parsedUserId]
                );
                return BadRequest(responseLocalizer["OidcEmailClaimMissingError"].Value);
            }

            var currentUser = await userManager.FindByIdAsync(parsedUserId.ToString());
            if (currentUser == null)
            {
                return NotFound(responseLocalizer["UserNotFound"].Value);
            }

            if (
                string.IsNullOrWhiteSpace(currentUser.Email)
                || !string.Equals(currentUser.Email, oidcEmail, StringComparison.OrdinalIgnoreCase)
            )
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer["OidcConnectEmailMismatchLog", parsedUserId]
                );
                return Conflict(responseLocalizer["OidcEmailMismatchError"].Value);
            }

            var providerKey =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                return BadRequest(responseLocalizer["LoginFailed"].Value);
            }

            await applicationUserService.ConnectOidcLoginAsync(
                parsedUserId,
                providerKey,
                userManager
            );
            return Ok();
        });
    }
}
