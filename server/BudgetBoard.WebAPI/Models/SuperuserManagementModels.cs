using System.ComponentModel.DataAnnotations;

namespace BudgetBoard.WebAPI.Models;

public sealed class CreateManagedUserRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }
}

public sealed record ManagedUserResponse(
    Guid Id,
    string Username,
    string? Email,
    bool HasPassword,
    bool HasOidcLogin,
    bool IsDisabled,
    bool MustChangePassword,
    bool IsSuperuser
);

public sealed record ManagedUserOperationResponse(
    ManagedUserResponse User,
    string? TemporaryPassword
);

public sealed record SuperuserOperationResult(
    bool Succeeded,
    string? ErrorCode,
    ManagedUserResponse? User = null,
    string? TemporaryPassword = null
)
{
    public static SuperuserOperationResult Failure(string errorCode) => new(false, errorCode);

    public static SuperuserOperationResult Success(
        ManagedUserResponse user,
        string? temporaryPassword = null
    ) => new(true, null, user, temporaryPassword);
}
