using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutomaticTransactionCategorizerController(
    ILogger<AutomaticTransactionCategorizerController> logger,
    UserManager<ApplicationUser> userManager,
    IAutomaticTransactionCategorizerService automaticTransactionCategorizerService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
)
    : ApiControllerBase<AutomaticTransactionCategorizerController>(
        logger,
        logLocalizer,
        responseLocalizer
    )
{
    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Train([FromBody] TrainAutoCategorizerRequest trainRequest)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await automaticTransactionCategorizerService.TrainCategorizerAsync(
                parsedUserId,
                trainRequest
            );
            return Ok();
        });
    }
}
