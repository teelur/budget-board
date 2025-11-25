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
public class AutomaticRuleController(
    ILogger<AutomaticRuleController> logger,
    UserManager<ApplicationUser> userManager,
    IAutomaticRuleService automaticRuleService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    private readonly ILogger<AutomaticRuleController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAutomaticRuleService _automaticRuleService = automaticRuleService;
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;
    private readonly IStringLocalizer<ApiResponseStrings> _responseLocalizer = responseLocalizer;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AutomaticRuleCreateRequest automaticRule)
    {
        try
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(_responseLocalizer["UserIdNotFound"].Value);
            }
            await _automaticRuleService.CreateAutomaticRuleAsync(
                new Guid(userIdString),
                automaticRule
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
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await _automaticRuleService.ReadAutomaticRulesAsync(
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
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AutomaticRuleUpdateRequest automaticRule)
    {
        try
        {
            await _automaticRuleService.UpdateAutomaticRuleAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                automaticRule
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
            await _automaticRuleService.DeleteAutomaticRuleAsync(
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
    public async Task<IActionResult> Run([FromBody] AutomaticRuleCreateRequest automaticRule)
    {
        try
        {
            return Ok(
                await _automaticRuleService.RunAutomaticRuleAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    automaticRule
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
}
