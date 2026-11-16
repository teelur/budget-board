using System.ComponentModel.DataAnnotations;

namespace BudgetBoard.WebAPI.Models;

public sealed class AdminAuditEventsQuery
{
    [Range(1, 1_000_000)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;

    [MaxLength(64)]
    public string? Action { get; set; }

    public bool? Succeeded { get; set; }

    [MaxLength(128)]
    public string? UserName { get; set; }
}

public sealed record AdminAuditEventResponse(
    long Id,
    Guid? ActorUserId,
    string? ActorUserName,
    Guid? TargetUserId,
    string? TargetUserName,
    string Action,
    bool Succeeded,
    DateTimeOffset CreatedAt,
    string? RequestIpAddress,
    string? Details
);

public sealed record AdminAuditEventsPageResponse(
    IReadOnlyList<AdminAuditEventResponse> Events,
    IReadOnlyList<string> Actions,
    int Page,
    int PageSize,
    int TotalCount
);

public sealed class CreateManagedUserRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }
}

public sealed class UpdateManagedUserUsernameRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
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
