using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
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
            return Ok(
                await applicationUserService.ReadApplicationUserAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty),
                    userManager
                )
            );
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] ApplicationUserUpdateRequest newUser)
    {
        return await HandleRequestAsync(async () =>
        {
            await applicationUserService.UpdateApplicationUserAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                newUser
            );
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
            await applicationUserService.DisconnectOidcLoginAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                userManager
            );
            return Ok();
        });
    }
}
