using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

public abstract class ApiControllerBase<TController>(
    ILogger<TController> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    private protected ILogger<TController> Logger { get; } = logger;

    private protected IStringLocalizer<ApiLogStrings> LogLocalizer { get; } = logLocalizer;

    private protected IStringLocalizer<ApiResponseStrings> ResponseLocalizer { get; } =
        responseLocalizer;

    protected async Task<IActionResult> HandleRequestAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{LogMessage}", LogLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(ResponseLocalizer["UnexpectedServerError"]);
        }
    }
}
