using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SimpleFinController(
    ILogger<SimpleFinController> logger,
    UserManager<ApplicationUser> userManager,
    ISimpleFinService simpleFinService
) : ControllerBase
{
    private readonly ILogger<SimpleFinController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ISimpleFinService _simpleFinService = simpleFinService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Sync()
    {
        try
        {
            return Ok(
                await _simpleFinService.SyncAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty)
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
    public async Task<IActionResult> UpdateAccessToken(string setupToken)
    {
        try
        {
            await _simpleFinService.UpdateAccessTokenFromSetupToken(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                setupToken
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
}
