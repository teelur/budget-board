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
public class BalanceController(
    ILogger<BalanceController> logger,
    UserManager<ApplicationUser> userManager,
    IBalanceService balanceService
) : ControllerBase
{
    private readonly ILogger<BalanceController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IBalanceService _balanceService = balanceService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] BalanceCreateRequest balance)
    {
        try
        {
            await _balanceService.CreateBalancesAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                balance
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read(Guid accountId)
    {
        try
        {
            return Ok(
                await _balanceService.ReadBalancesAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    accountId
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] BalanceUpdateRequest updatedBalance)
    {
        try
        {
            await _balanceService.UpdateBalanceAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                updatedBalance
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _balanceService.DeleteBalanceAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                id
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }
}
