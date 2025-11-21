using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.WebAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace BudgetBoard.WebAPI.Services;

public class OidcTokenService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<IOidcTokenService> logger
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
    public async Task<ClaimsPrincipal?> ExchangeCodeForUserAsync(
        string authorizationCode,
        string redirectUri
    )
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
                clientSecret,
                redirectUri
            );

            if (tokenResponse?.IdToken == null)
            {
                _logger.LogError("Token exchange failed or no ID token received");
                return null;
            }

            // Parse and validate ID token
            var principal = await ValidateIdTokenAsync(
                tokenResponse.IdToken,
                authority,
                clientId,
                discoveryDoc.JwksUri
            );
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
    private async Task<OidcDiscoveryDocument?> GetDiscoveryDocumentAsync(string authority)
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

            var discoveryDoc = JsonSerializer.Deserialize<OidcDiscoveryDocument>(
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
    private async Task<OidcTokenResponse?> ExchangeCodeForTokensAsync(
        string tokenEndpoint,
        string authorizationCode,
        string clientId,
        string clientSecret,
        string redirectUri
    )
    {
        try
        {
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "authorization_code"),
                new("code", authorizationCode),
                new("redirect_uri", redirectUri),
            };

            using var content = new FormUrlEncodedContent(tokenRequest);

            // Use Basic authentication for client credentials
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")
            );

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = content,
            };
            request.Headers.Add("Authorization", $"Basic {credentials}");

            var response = await _httpClient.SendAsync(request);

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
            var tokenResponse = JsonSerializer.Deserialize<OidcTokenResponse>(
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

    /// <summary>
    /// Validates the ID token using proper JWT validation with signature, issuer, audience, and expiration checks.
    /// </summary>
    /// <param name="idToken">The ID token to validate.</param>
    /// <param name="authority">The OIDC authority/issuer.</param>
    /// <param name="clientId">The client ID (audience).</param>
    /// <param name="jwksUri">The JWKS URI to fetch signing keys.</param>
    /// <returns>A validated ClaimsPrincipal, or null if validation fails.</returns>
    private async Task<ClaimsPrincipal?> ValidateIdTokenAsync(
        string idToken,
        string authority,
        string clientId,
        string? jwksUri
    )
    {
        try
        {
            if (string.IsNullOrEmpty(jwksUri))
            {
                _logger.LogError("JWKS URI is missing from discovery document");
                return null;
            }

            // Fetch signing keys from the JWKS endpoint
            var signingKeys = await GetSigningKeysAsync(jwksUri);
            if (signingKeys == null || signingKeys.Count == 0)
            {
                _logger.LogError("Failed to retrieve signing keys from JWKS endpoint");
                return null;
            }

            // Configure token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                // Allow for some clock skew
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                idToken,
                validationParameters,
                out SecurityToken validatedToken
            );

            // Ensure we have the required claims for user provisioning
            var subClaim = principal.FindFirst(c =>
                c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier
            );
            var emailClaim = principal.FindFirst(c =>
                c.Type == "email" || c.Type == ClaimTypes.Email
            );

            if (subClaim == null || emailClaim == null)
            {
                _logger.LogError("Required claims (sub and email) not found in ID token");
                return null;
            }

            // Add standard claim types if they don't exist
            var identity = (ClaimsIdentity)principal.Identity!;
            if (!principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            if (!principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
            }

            // Add name claim if available
            var nameClaim = principal.FindFirst(c =>
                c.Type == "name" || c.Type == "preferred_username"
            );
            if (nameClaim != null && !principal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
            }

            _logger.LogInformation("ID token validated successfully");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogError(ex, "ID token has expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogError(ex, "ID token has invalid signature");
            return null;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogError(ex, "ID token has invalid issuer");
            return null;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogError(ex, "ID token has invalid audience");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ID token");
            return null;
        }
    }

    /// <summary>
    /// Fetches the signing keys from the JWKS endpoint.
    /// </summary>
    /// <param name="jwksUri">The JWKS URI.</param>
    /// <returns>A collection of security keys, or null if retrieval fails.</returns>
    private async Task<ICollection<SecurityKey>?> GetSigningKeysAsync(string jwksUri)
    {
        try
        {
            _logger.LogInformation("Fetching signing keys from: {JwksUri}", jwksUri);

            var response = await _httpClient.GetAsync(jwksUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jwks = new JsonWebKeySet(content);

            if (jwks.Keys == null || !jwks.Keys.Any())
            {
                _logger.LogError("No signing keys found in JWKS response");
                return null;
            }

            _logger.LogInformation(
                "Successfully retrieved {Count} signing key(s)",
                jwks.Keys.Count
            );
            return jwks.Keys.Cast<SecurityKey>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signing keys from {JwksUri}", jwksUri);
            return null;
        }
    }
}
