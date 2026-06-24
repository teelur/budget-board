using BudgetBoard.Database.Models;
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
public class AssetTypeController(
    ILogger<AssetTypeController> logger,
    UserManager<ApplicationUser> userManager,
    IAssetTypeService assetTypeService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<AssetTypeController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AssetTypeCreateRequest newAssetType)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await assetTypeService.CreateAssetTypeAsync(parsedUserId, newAssetType);
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

            return Ok(await assetTypeService.ReadAssetTypesAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AssetTypeUpdateRequest updatedAssetType)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await assetTypeService.UpdateAssetTypeAsync(parsedUserId, updatedAssetType);
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid assetId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await assetTypeService.DeleteAssetTypeAsync(parsedUserId, assetId);
            return Ok();
        });
    }
}
