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
public class InstitutionController(
    ILogger<InstitutionController> logger,
    UserManager<ApplicationUser> userManager,
    IInstitutionService institutionService
) : ControllerBase
{
    private readonly ILogger<InstitutionController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IInstitutionService _institutionService = institutionService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] InstitutionCreateRequest createdInstitution)
    {
        try
        {
            await _institutionService.CreateInstitutionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                createdInstitution
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
                await _institutionService.ReadInstitutionsAsync(
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

    [HttpGet("{guid}")]
    [Authorize]
    public async Task<IActionResult> Read(Guid guid)
    {
        try
        {
            return Ok(
                await _institutionService.ReadInstitutionsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    guid
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
    public async Task<IActionResult> Update([FromBody] InstitutionUpdateRequest updatedInstitution)
    {
        try
        {
            await _institutionService.UpdateInstitutionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                updatedInstitution
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

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid guid, bool deleteTransactions = false)
    {
        try
        {
            await _institutionService.DeleteInstitutionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                guid,
                deleteTransactions
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

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> SetIndices(
        [FromBody] List<InstitutionIndexRequest> institutions
    )
    {
        try
        {
            await _institutionService.OrderInstitutionsAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                institutions
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
