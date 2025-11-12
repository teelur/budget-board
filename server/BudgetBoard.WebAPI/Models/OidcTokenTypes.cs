using System.Text.Json.Serialization;

namespace BudgetBoard.WebAPI.Models;

/// <summary>
/// Represents the OpenID Connect Discovery Document
/// </summary>
public class DiscoveryDocument
{
    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }

    [JsonPropertyName("authorization_endpoint")]
    public string? AuthorizationEndpoint { get; set; }

    [JsonPropertyName("token_endpoint")]
    public string? TokenEndpoint { get; set; }

    [JsonPropertyName("userinfo_endpoint")]
    public string? UserInfoEndpoint { get; set; }

    [JsonPropertyName("jwks_uri")]
    public string? JwksUri { get; set; }

    [JsonPropertyName("end_session_endpoint")]
    public string? EndSessionEndpoint { get; set; }

    [JsonPropertyName("registration_endpoint")]
    public string? RegistrationEndpoint { get; set; }

    [JsonPropertyName("scopes_supported")]
    public string[]? ScopesSupported { get; set; }

    [JsonPropertyName("response_types_supported")]
    public string[]? ResponseTypesSupported { get; set; }

    [JsonPropertyName("response_modes_supported")]
    public string[]? ResponseModesSupported { get; set; }

    [JsonPropertyName("grant_types_supported")]
    public string[]? GrantTypesSupported { get; set; }

    [JsonPropertyName("subject_types_supported")]
    public string[]? SubjectTypesSupported { get; set; }

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[]? IdTokenSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public string[]? TokenEndpointAuthMethodsSupported { get; set; }

    [JsonPropertyName("claims_supported")]
    public string[]? ClaimsSupported { get; set; }
}

/// <summary>
/// Represents the token response from the token endpoint
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}
