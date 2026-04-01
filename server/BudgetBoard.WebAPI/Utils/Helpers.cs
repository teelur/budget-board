using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.Utils;

public static class Helpers
{
    public const long UNIX_MONTH = 2629743;
    public const long UNIX_WEEK = 604800;

    public const string DEFAULT_ERROR_STRING = "There was an internal server error.";

    public static IActionResult BuildErrorResponse(string message = DEFAULT_ERROR_STRING)
    {
        var errorObjectResult = new ObjectResult(message)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };

        return errorObjectResult;
    }

    public static HostString GetHostString(HttpRequest request)
    {
        var host = GetHost(request);
        var port = GetPort(request);

        if (port == -1)
        {
            return new HostString(host);
        }
        else
        {
            return new HostString(host, port);
        }
    }

    public static string GetHost(HttpRequest request)
    {
        return request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.ToString();
    }

    public static int GetPort(HttpRequest request)
    {
        var portString = request.Headers["X-Forwarded-Port"].FirstOrDefault() ?? "-1";
        return int.Parse(portString);
    }

    public static string GetProto(HttpRequest request)
    {
        var protoHeader = request.Headers["X-Forwarded-Proto"].FirstOrDefault();

        if (!string.IsNullOrEmpty(protoHeader))
        {
            // X-Forwarded-Proto can contain a comma-separated list (e.g. "https,http").
            // Take the first non-empty, trimmed token.
            var firstToken = protoHeader.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(firstToken))
            {
                return firstToken;
            }
        }

        return request.Scheme;
    }
}
