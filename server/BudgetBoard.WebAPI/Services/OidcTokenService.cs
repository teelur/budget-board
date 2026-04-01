using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace BudgetBoard.WebAPI.Services;

public class OidcTokenService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<IOidcTokenService> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer
) : IOidcTokenService
{
    private readonly HttpClient _httpClient =
        httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<IOidcTokenService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;

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
            var authority = _configuration.GetValue<string>("OIDC_ISSUER");
            var clientId = _configuration.GetValue<string>("OIDC_CLIENT_ID");
            var clientSecret = _configuration.GetValue<string>("OIDC_CLIENT_SECRET");

            if (
                string.IsNullOrEmpty(authority)
                || string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
            )
            {
                _logger.LogError("{LogMessage}", _logLocalizer["OidcConfigurationMissingLog"]);
                return null;
            }

            var discoveryDoc = await GetDiscoveryDocumentAsync(authority);
            if (discoveryDoc == null || string.IsNullOrEmpty(discoveryDoc.TokenEndpoint))
            {
                _logger.LogError(
                    "{LogMessage}",
                    _logLocalizer["OidcTokenEndpointDiscoveryErrorLog"]
                );
                return null;
            }

            var tokenResponse = await ExchangeCodeForTokensAsync(
                discoveryDoc.TokenEndpoint,
                authorizationCode,
                clientId,
                clientSecret,
                redirectUri
            );

            if (tokenResponse?.IdToken == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["OidcTokenExchangeFailedLog"]);
                return null;
            }

            return await ValidateIdTokenAsync(
                tokenResponse.IdToken,
                authority,
                clientId,
                discoveryDoc.JwksUri
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
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
                "{LogMessage}",
                _logLocalizer["FetchingDiscoveryDocumentLog", discoveryUrl]
            );

            var response = await _httpClient.GetAsync(discoveryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("{LogMessage}", _logLocalizer["DiscoveryDocumentContentLog", content]);

            var discoveryDoc = JsonSerializer.Deserialize<OidcDiscoveryDocument>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (discoveryDoc != null)
            {
                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer[
                        "DiscoveryDocumentParsedLog",
                        discoveryDoc.TokenEndpoint ?? string.Empty,
                        discoveryDoc.Issuer ?? string.Empty
                    ]
                );
            }
            else
            {
                _logger.LogError(
                    "{LogMessage}",
                    _logLocalizer["OidcDiscoveryDocumentDeserializationErrorLog"]
                );
            }

            return discoveryDoc;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{LogMessage}",
                _logLocalizer["DiscoveryDocumentFetchErrorLog", authority]
            );
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
                    "{LogMessage}",
                    _logLocalizer["TokenExchangeErrorLog", response.StatusCode, errorContent]
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
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["TokenExchangeRequestErrorLog"]);
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
                _logger.LogError("{LogMessage}", _logLocalizer["OidcJwksUriMissingLog"]);
                return null;
            }

            // Fetch signing keys from the JWKS endpoint
            var signingKeys = await GetSigningKeysAsync(jwksUri);
            if (signingKeys == null || signingKeys.Count == 0)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["OidcSigningKeysRetrievalErrorLog"]);
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
                _logger.LogError("{LogMessage}", _logLocalizer["OidcRequiredClaimsMissingLog"]);
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

            _logger.LogInformation("{LogMessage}", _logLocalizer["IdTokenValidatedLog"]);
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["IdTokenExpiredLog"]);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["IdTokenInvalidSignatureLog"]);
            return null;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["IdTokenInvalidIssuerLog"]);
            return null;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["IdTokenInvalidAudienceLog"]);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["IdTokenValidationErrorLog"]);
            return null;
        }
    }

    private async Task<ICollection<SecurityKey>?> GetSigningKeysAsync(string jwksUri)
    {
        try
        {
            _logger.LogInformation(
                "{LogMessage}",
                _logLocalizer["FetchingSigningKeysLog", jwksUri]
            );

            var response = await _httpClient.GetAsync(jwksUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jwks = new JsonWebKeySet(content);

            if (jwks.Keys == null || !jwks.Keys.Any())
            {
                _logger.LogError("{LogMessage}", _logLocalizer["OidcNoSigningKeysFoundLog"]);
                return null;
            }

            _logger.LogInformation(
                "{LogMessage}",
                _logLocalizer["SigningKeysRetrievalSuccessLog", jwks.Keys.Count]
            );
            return jwks.Keys.Cast<SecurityKey>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{LogMessage}",
                _logLocalizer["SigningKeysFetchErrorLog", jwksUri]
            );
            return null;
        }
    }
}
