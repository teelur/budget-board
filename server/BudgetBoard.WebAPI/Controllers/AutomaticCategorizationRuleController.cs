using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutomaticCategorizationRuleController(
    ILogger<AutomaticCategorizationRuleController> logger,
    UserManager<ApplicationUser> userManager,
    IAutomaticCategorizationRuleService automaticCategorizationRuleService
) : ControllerBase
{
    private readonly ILogger<AutomaticCategorizationRuleController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAutomaticCategorizationRuleService _automaticCategorizationRuleService =
        automaticCategorizationRuleService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] AutomaticCategorizationRuleCreateRequest automaticCategorizationRule
    )
    {
        try
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("User ID not found.");
            }
            await _automaticCategorizationRuleService.CreateAutomaticCategorizationRuleAsync(
                new Guid(userIdString),
                automaticCategorizationRule
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
    [Authorize]
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await _automaticCategorizationRuleService.ReadAutomaticCategorizationRulesAsync(
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

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid guid)
    {
        try
        {
            await _automaticCategorizationRuleService.DeleteAutomaticCategorizationRuleAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                guid
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
