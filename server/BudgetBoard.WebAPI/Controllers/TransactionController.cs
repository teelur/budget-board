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
public class TransactionController(
    ILogger<TransactionController> logger,
    UserManager<ApplicationUser> userManager,
    ITransactionService transactionService
) : ControllerBase
{
    private readonly ILogger<TransactionController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITransactionService _transactionService = transactionService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] TransactionCreateRequest transaction)
    {
        try
        {
            await _transactionService.CreateTransactionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                transaction
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
    public async Task<IActionResult> Read(int? year, int? month, bool getHidden = false)
    {
        try
        {
            return Ok(
                await _transactionService.ReadTransactionsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    year,
                    month,
                    getHidden
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

    [HttpGet("{guid}")]
    [Authorize]
    public async Task<IActionResult> Read(Guid guid)
    {
        try
        {
            return Ok(
                await _transactionService.ReadTransactionsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    null,
                    null,
                    false,
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
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] TransactionUpdateRequest newTransaction)
    {
        try
        {
            await _transactionService.UpdateTransactionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                newTransaction
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
    public async Task<IActionResult> Delete(Guid guid)
    {
        try
        {
            await _transactionService.DeleteTransactionAsync(
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
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Restore(Guid guid)
    {
        try
        {
            await _transactionService.RestoreTransactionAsync(
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
            _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex);
            return Helpers.BuildErrorResponse("An unexpected server error occurred.");
        }
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Split([FromBody] TransactionSplitRequest splitTransaction)
    {
        try
        {
            await _transactionService.SplitTransactionAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                splitTransaction
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

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Import([FromBody] TransactionImportRequest importTransaction)
    {
        try
        {
            await _transactionService.ImportTransactionsAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                importTransaction
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
