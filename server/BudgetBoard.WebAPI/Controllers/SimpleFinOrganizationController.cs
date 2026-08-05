using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SimpleFinOrganizationController(
    ILogger<SimpleFinOrganizationController> logger,
    UserManager<ApplicationUser> userManager,
    ISimpleFinOrganizationService simpleFinOrganizationService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<SimpleFinOrganizationController>(logger, logLocalizer, responseLocalizer)
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
                await simpleFinOrganizationService.ReadSimpleFinOrganizationsAsync(parsedUserId)
            );
        });
    }
}
