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
public class InstitutionController(
    ILogger<InstitutionController> logger,
    UserManager<ApplicationUser> userManager,
    IInstitutionService institutionService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<InstitutionController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] InstitutionCreateRequest createdInstitution)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await institutionService.CreateInstitutionAsync(parsedUserId, createdInstitution);
            return Ok();
        });
    }

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

            return Ok(await institutionService.ReadInstitutionsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] InstitutionUpdateRequest updatedInstitution)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await institutionService.UpdateInstitutionAsync(parsedUserId, updatedInstitution);
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Order(
        [FromBody] List<InstitutionIndexRequest> orderedInstitutions
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await institutionService.OrderInstitutionsAsync(parsedUserId, orderedInstitutions);
            return Ok();
        });
    }
}
