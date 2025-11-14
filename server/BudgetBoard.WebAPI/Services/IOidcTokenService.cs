using System.Security.Claims;

namespace BudgetBoard.WebAPI.Services;

public interface IOidcTokenService
{
    Task<ClaimsPrincipal?> ExchangeCodeForUserAsync(string authorizationCode, string redirectUri);
}
