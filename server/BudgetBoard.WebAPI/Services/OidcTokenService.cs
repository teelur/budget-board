using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.WebAPI.Models;

namespace BudgetBoard.WebAPI.Services;

public class OidcTokenService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OidcTokenService> logger
) : IOidcTokenService
{
    private readonly HttpClient _httpClient =
        httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<IOidcTokenService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Exchanges the authorization code for user claims by communicating with the OIDC provider.
    /// </summary>
    /// <param name="authorizationCode">The authorization code received from the OIDC provider.</param>
    /// <returns>A ClaimsPrincipal representing the user, or null if the exchange fails.</returns>
    public async Task<ClaimsPrincipal?> ExchangeCodeForUserAsync(string authorizationCode)
    {
        try
        {
            // Get OIDC configuration
            var authority = _configuration.GetValue<string>("OIDC_ISSUER");
            var clientId = _configuration.GetValue<string>("OIDC_CLIENT_ID");
            var clientSecret = _configuration.GetValue<string>("OIDC_CLIENT_SECRET");

            if (
                string.IsNullOrEmpty(authority)
                || string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
            )
            {
                _logger.LogError("OIDC configuration is missing");
                return null;
            }

            // Discover token endpoint
            var discoveryDoc = await GetDiscoveryDocumentAsync(authority);
            if (discoveryDoc?.TokenEndpoint == null)
            {
                _logger.LogError("Could not discover token endpoint");
                return null;
            }

            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeCodeForTokensAsync(
                discoveryDoc.TokenEndpoint,
                authorizationCode,
                clientId,
                clientSecret
            );

            if (tokenResponse?.IdToken == null)
            {
                _logger.LogError("Token exchange failed or no ID token received");
                return null;
            }

            // Parse and validate ID token
            var principal = ParseIdToken(tokenResponse.IdToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token exchange");
            return null;
        }
    }

    /// <summary>
    /// Retrieves the OIDC discovery document from the authority.
    /// </summary>
    /// <param name="authority">The OIDC authority URL.</param>
    /// <returns>The discovery document, or null if retrieval fails.</returns>
    private async Task<DiscoveryDocument?> GetDiscoveryDocumentAsync(string authority)
    {
        try
        {
            var discoveryUrl = $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
            _logger.LogInformation(
                "Fetching discovery document from: {DiscoveryUrl}",
                discoveryUrl
            );

            var response = await _httpClient.GetAsync(discoveryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Discovery document content: {Content}", content);

            var discoveryDoc = JsonSerializer.Deserialize<DiscoveryDocument>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (discoveryDoc != null)
            {
                _logger.LogInformation(
                    "Discovery document parsed. TokenEndpoint: {TokenEndpoint}, Issuer: {Issuer}",
                    discoveryDoc.TokenEndpoint,
                    discoveryDoc.Issuer
                );
            }
            else
            {
                _logger.LogError("Failed to deserialize discovery document");
            }

            return discoveryDoc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get discovery document from {Authority}", authority);
            return null;
        }
    }

    /// <summary>
    /// Exchanges the authorization code for tokens at the token endpoint.
    /// </summary>
    /// <param name="tokenEndpoint">The token endpoint URL.</param>
    /// <param name="authorizationCode">The authorization code.</param>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <returns>The token response, or null if the exchange fails.</returns>
    private async Task<TokenResponse?> ExchangeCodeForTokensAsync(
        string tokenEndpoint,
        string authorizationCode,
        string clientId,
        string clientSecret
    )
    {
        try
        {
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "authorization_code"),
                new("code", authorizationCode),
                new("client_id", clientId),
                new("client_secret", clientSecret),
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Token exchange failed with status {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent
                );
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token exchange request");
            return null;
        }
    }

    private ClaimsPrincipal? ParseIdToken(string idToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(idToken);

            // Extract claims from the token
            var claims = new List<Claim>();

            foreach (var claim in jwt.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            // Ensure we have the required claims for user provisioning
            var subClaim = claims.FirstOrDefault(c =>
                c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier
            );
            var emailClaim = claims.FirstOrDefault(c =>
                c.Type == "email" || c.Type == ClaimTypes.Email
            );

            if (subClaim == null || emailClaim == null)
            {
                _logger.LogError("Required claims (sub and email) not found in ID token");
                return null;
            }

            // Add standard claim types if they don't exist
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            if (!claims.Any(c => c.Type == ClaimTypes.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, emailClaim.Value));
            }

            // Add name claim if available
            var nameClaim = claims.FirstOrDefault(c =>
                c.Type == "name" || c.Type == "preferred_username"
            );
            if (nameClaim != null && !claims.Any(c => c.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            var identity = new ClaimsIdentity(claims, "oidc");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing ID token");
            return null;
        }
    }
}
