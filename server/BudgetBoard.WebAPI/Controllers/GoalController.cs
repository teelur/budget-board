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
public class GoalController(
    ILogger<GoalController> logger,
    UserManager<ApplicationUser> userManager,
    IGoalService goalService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    private readonly ILogger<GoalController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IGoalService _goalService = goalService;
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;
    private readonly IStringLocalizer<ApiResponseStrings> _responseLocalizer = responseLocalizer;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] GoalCreateRequest newGoal)
    {
        try
        {
            await _goalService.CreateGoalAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                newGoal
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read(bool includeInterest = false)
    {
        try
        {
            return Ok(
                await _goalService.ReadGoalsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    includeInterest
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] GoalUpdateRequest editedGoal)
    {
        try
        {
            await _goalService.UpdateGoalAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                editedGoal
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid guid)
    {
        try
        {
            await _goalService.DeleteGoalAsync(
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
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Complete(Guid goalID)
    {
        try
        {
            await _goalService.CompleteGoalAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                goalID,
                DateTime.UtcNow
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }
}
