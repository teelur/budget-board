﻿using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetController(ILogger<BudgetController> logger, UserManager<ApplicationUser> userManager, IBudgetService budgetService) : ControllerBase
{
    private readonly ILogger<BudgetController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IBudgetService _budgetService = budgetService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] BudgetCreateRequest[] budgets)
    {
        try
        {
            await _budgetService.CreateBudgetsAsync(new Guid(_userManager.GetUserId(User) ?? string.Empty), budgets);
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch
        {
            return Helpers.BuildErrorResponse();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read(DateTime date)
    {
        try
        {
            return Ok(await _budgetService.ReadBudgetsAsync(new Guid(_userManager.GetUserId(User) ?? string.Empty), date));
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch
        {
            return Helpers.BuildErrorResponse();
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] BudgetUpdateRequest editBudget)
    {
        try
        {
            await _budgetService.UpdateBudgetAsync(new Guid(_userManager.GetUserId(User) ?? string.Empty), editBudget);
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch
        {
            return Helpers.BuildErrorResponse();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid guid)
    {
        try
        {
            await _budgetService.DeleteBudgetAsync(new Guid(_userManager.GetUserId(User) ?? string.Empty), guid);
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch
        {
            return Helpers.BuildErrorResponse();
        }
    }
}
