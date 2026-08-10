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
public class TagController(
    ILogger<TagController> logger,
    UserManager<ApplicationUser> userManager,
    ITagService tagService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<TagController>(logger, logLocalizer, responseLocalizer)
{
    [HttpGet("suggestions")]
    [Authorize]
    public async Task<IActionResult> Suggestions(
        [FromQuery] string? prefix,
        [FromQuery] int limit = 20
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await tagService.ReadSuggestionsAsync(parsedUserId, prefix, limit));
        });
    }
}
