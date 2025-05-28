using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserSettingsController(
    ILogger<AccountController> logger,
    UserManager<ApplicationUser> userManager,
    IUserSettingsService userSettingsService
) : ControllerBase
{
    private readonly ILogger<AccountController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserSettingsService _userSettingsService = userSettingsService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await _userSettingsService.ReadUserSettingsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty)
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            var errorObjectResult = new ObjectResult(bbex.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            return errorObjectResult;
        }
        catch
        {
            var errorObjectResult = new ObjectResult("There was an internal server error.")
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            return errorObjectResult;
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] UserSettingsUpdateRequest userSettingsUpdateRequest
    )
    {
        try
        {
            await _userSettingsService.UpdateUserSettingsAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                userSettingsUpdateRequest
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            var errorObjectResult = new ObjectResult(bbex.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            return errorObjectResult;
        }
        catch
        {
            var errorObjectResult = new ObjectResult("There was an internal server error.")
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            return errorObjectResult;
        }
    }
}
