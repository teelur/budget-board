using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Utils;

public class DemoModeFilter(
    IConfiguration configuration,
    IStringLocalizer<ApiResponseStrings> localizer
) : IActionFilter
{
    private readonly bool _demoModeEnabled = configuration.GetValue<bool>("DEMO_MODE");

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_demoModeEnabled)
        {
            return;
        }

        var method = context.HttpContext.Request.Method.ToUpperInvariant();
        var path = (context.HttpContext.Request.Path.Value ?? string.Empty).ToLowerInvariant();

        if (IsBlocked(method, path))
        {
            context.Result = new ObjectResult(localizer["DemoModeNotAllowed"].Value)
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }
    }

    private static bool IsBlocked(string method, string path)
    {
        // Block entire external sync / integration controllers
        if (
            path.StartsWith("/api/simplefin")
            || path.StartsWith("/api/lunchflow")
            || path.StartsWith("/api/simplefinaccount")
            || path.StartsWith("/api/lunchflowaccount")
            || path.StartsWith("/api/oidc")
        )
        {
            return true;
        }

        // Block structural account mutations
        if (path == "/api/account" && (method is "POST" or "DELETE"))
            return true;
        if (path == "/api/account/restore" && method == "POST")
            return true;
        if (path == "/api/account/order" && method == "PUT")
            return true;
        if (path == "/api/account/permanentdelete" && method == "DELETE")
            return true;

        // Block structural institution mutations
        if (path == "/api/institution" && (method is "POST" or "DELETE"))
            return true;
        if (path == "/api/institution/setindices" && method == "PUT")
            return true;

        // Block bulk transaction import
        if (path == "/api/transaction/import" && method == "POST")
            return true;

        // Block OIDC login disconnect
        if (path == "/api/applicationuser/disconnectoidclogin" && method == "DELETE")
            return true;

        return false;
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
