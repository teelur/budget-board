using System.ComponentModel.DataAnnotations;

namespace BudgetBoard.WebAPI.Models;

public sealed class LocalRegisterRequest
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class LocalLoginRequest
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Identifier { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? TwoFactorCode { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("recoveryCode")]
    public string? TwoFactorRecoveryCode { get; set; }

    public string GetIdentifier() =>
        !string.IsNullOrWhiteSpace(Identifier)
            ? Identifier.Trim()
            : !string.IsNullOrWhiteSpace(Username)
                ? Username.Trim()
                : Email?.Trim() ?? string.Empty;
}
