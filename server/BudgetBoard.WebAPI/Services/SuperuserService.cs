using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class SuperuserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserDataContext dbContext,
    IWidgetSettingsService widgetSettingsService,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SuperuserService> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> localizer
) : ISuperuserService
{
    private const int TemporaryPasswordByteLength = 18;
    private const long FirstUserBootstrapLockKey = 0x4242555341464555L;

    /// <inheritdoc />
    public async Task<TResult> ExecuteFirstUserBootstrapAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!dbContext.Database.IsNpgsql())
        {
            return await operation();
        }

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(
            async executionCancellationToken =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    executionCancellationToken
                );
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock({FirstUserBootstrapLockKey})",
                    executionCancellationToken
                );
                var result = await operation();
                await transaction.CommitAsync(executionCancellationToken);
                return result;
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task EnsureRoleAsync(CancellationToken cancellationToken = default)
    {
        if (await roleManager.FindByNameAsync(SuperuserConstants.RoleName) is not null)
        {
            return;
        }

        var createRoleResult = await roleManager.CreateAsync(
            new IdentityRole<Guid>(SuperuserConstants.RoleName)
        );
        if (!createRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                localizer[
                    "SuperuserRoleCreationFailed",
                    SuperuserConstants.RoleName,
                    string.Join(", ", createRoleResult.Errors.Select(error => error.Code))
                ].Value
            );
        }
    }

    /// <inheritdoc />
    public async Task EnsureRoleAndBootstrapAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync(cancellationToken);

        var superusers = await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName);
        if (superusers.Count > 1)
        {
            throw new InvalidOperationException(
                localizer["MultipleSuperusersDetected", SuperuserConstants.RoleName].Value
            );
        }

        if (superusers.Count == 1)
        {
            return;
        }

        var users = await userManager.Users.ToListAsync(cancellationToken);
        if (users.Count == 0)
        {
            return;
        }

        var selector = configuration[SuperuserConstants.BootstrapUsernameConfigurationKey]?.Trim();
        ApplicationUser? target;

        if (users.Count == 1)
        {
            target = users[0];
        }
        else
        {
            if (string.IsNullOrWhiteSpace(selector))
            {
                throw new InvalidOperationException(
                    localizer[
                        "SuperuserBootstrapSelectionRequired",
                        SuperuserConstants.BootstrapUsernameConfigurationKey
                    ].Value
                );
            }

            target =
                users.SingleOrDefault(user =>
                    string.Equals(user.UserName, selector, StringComparison.OrdinalIgnoreCase)
                )
                ?? throw new InvalidOperationException(
                    localizer[
                        "SuperuserBootstrapUserNotFound",
                        SuperuserConstants.BootstrapUsernameConfigurationKey
                    ].Value
                );
        }

        var result = await userManager.AddToRoleAsync(target, SuperuserConstants.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                localizer[
                    "SuperuserPromotionFailed",
                    string.Join(", ", result.Errors.Select(error => error.Code))
                ].Value
            );
        }

        await userManager.UpdateSecurityStampAsync(target);
        await AuditAsync(null, target.Id, "bootstrap-superuser", true, null, cancellationToken);

        logger.LogInformation(
            "{LogMessage}",
            logLocalizer[
                "SuperuserBootstrapPromotionLog",
                target.Id,
                SuperuserConstants.RoleName
            ]
        );
    }

    /// <inheritdoc />
    public async Task EnsureFirstUserSuperuserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default
    )
    {
        var superusers = await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName);
        if (superusers.Count > 0)
        {
            return;
        }

        var userCount = await userManager.Users.CountAsync(cancellationToken);
        if (userCount != 1)
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(user, SuperuserConstants.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                localizer[
                    "FirstUserPromotionFailed",
                    string.Join(", ", result.Errors.Select(error => error.Code))
                ].Value
            );
        }

        await userManager.UpdateSecurityStampAsync(user);
        await AuditAsync(null, user.Id, "bootstrap-first-user", true, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ManagedUserResponse>> ListUsersAsync(
        CancellationToken cancellationToken = default
    )
    {
        var superuserIds = (await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName))
            .Select(user => user.Id)
            .ToHashSet();
        var users = await userManager
            .Users.OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);
        var responses = new List<ManagedUserResponse>(users.Count);

        foreach (var user in users)
        {
            responses.Add(await ToResponseAsync(user, superuserIds.Contains(user.Id)));
        }

        return responses;
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> CreateUserAsync(
        CreateManagedUserRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    )
    {
        var username = request.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            return await FailureAsync("UsernameRequired", "create-user", null, actorUserId);
        }

        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            email = null;
        }
        if (!string.IsNullOrEmpty(email) && !new EmailAddressAttribute().IsValid(email))
        {
            return await FailureAsync("InvalidEmail", "create-user", null, actorUserId);
        }

        if (await userManager.FindByNameAsync(username) is not null)
        {
            return await FailureAsync("UsernameAlreadyTaken", "create-user", null, actorUserId);
        }

        var temporaryPassword = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = email is not null,
            MustChangePassword = true,
        };

        var createResult = await userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            return await FailureAsync("UserCreationFailed", "create-user", null, actorUserId);
        }

        var addLoginResult = await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(
                SuperuserConstants.LocalLoginProvider,
                user.Id.ToString(),
                localizer["LocalAccountProviderName"].Value
            )
        );
        if (!addLoginResult.Succeeded)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["ManagedUserLocalLoginMetadataWarningLog", user.Id]
            );
        }

        try
        {
            foreach (var layout in WidgetSettingsHelpers.DefaultLayouts)
            {
                await widgetSettingsService.CreateWidgetSettingsAsync(
                    user.Id,
                    new WidgetSettingsCreateRequest
                    {
                        WidgetType = layout.WidgetType,
                        LgX = layout.LgX,
                        LgY = layout.LgY,
                        LgW = layout.LgW,
                        LgH = layout.LgH,
                        SmY = layout.SmY,
                        SmH = layout.SmH,
                    }
                );
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "{LogMessage}",
                logLocalizer["ManagedUserInitializationFailedLog", user.Id]
            );
            await userManager.DeleteAsync(user);
            return await FailureAsync(
                "DefaultWidgetSeedingFailed",
                "create-user",
                user.Id,
                actorUserId
            );
        }

        await AuditAsync(actorUserId, user.Id, "create-user", true, null, cancellationToken);
        return SuperuserOperationResult.Success(
            await ToResponseAsync(user, false),
            temporaryPassword
        );
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> ResetPasswordAsync(
        Guid targetUserId,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    )
    {
        var target = await userManager.FindByIdAsync(targetUserId.ToString());
        if (target is null)
        {
            return await FailureAsync("UserNotFound", "reset-password", targetUserId, actorUserId);
        }

        return await ResetPasswordCoreAsync(
            target,
            "reset-password",
            actorUserId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> SetDisabledAsync(
        Guid targetUserId,
        Guid actorUserId,
        bool disabled,
        CancellationToken cancellationToken = default
    )
    {
        var target = await userManager.FindByIdAsync(targetUserId.ToString());
        if (target is null)
        {
            return await FailureAsync("UserNotFound", "set-disabled", targetUserId, actorUserId);
        }

        if (target.Id == actorUserId)
        {
            return await FailureAsync(
                "SelfDisableNotAllowed",
                "set-disabled",
                target.Id,
                actorUserId
            );
        }

        if (await userManager.IsInRoleAsync(target, SuperuserConstants.RoleName))
        {
            return await FailureAsync(
                "SuperuserDisableNotAllowed",
                "set-disabled",
                target.Id,
                actorUserId
            );
        }

        target.IsDisabled = disabled;
        var updateResult = await userManager.UpdateAsync(target);
        if (!updateResult.Succeeded)
        {
            return await FailureAsync("UserUpdateFailed", "set-disabled", target.Id, actorUserId);
        }

        await userManager.UpdateSecurityStampAsync(target);
        await AuditAsync(
            actorUserId,
            target.Id,
            disabled ? "disable-user" : "enable-user",
            true,
            null,
            cancellationToken
        );
        return SuperuserOperationResult.Success(
            await ToResponseAsync(
                target,
                await userManager.IsInRoleAsync(target, SuperuserConstants.RoleName)
            )
        );
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> TransferSuperuserAsync(
        Guid targetUserId,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    )
    {
        var target = await userManager.FindByIdAsync(targetUserId.ToString());
        if (target is null)
        {
            return await FailureAsync(
                "UserNotFound",
                "transfer-superuser",
                targetUserId,
                actorUserId
            );
        }

        if (target.IsDisabled)
        {
            return await FailureAsync(
                "DisabledUserCannotBeSuperuser",
                "transfer-superuser",
                target.Id,
                actorUserId
            );
        }

        var currentSuperusers = await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName);
        if (currentSuperusers.Count != 1)
        {
            return await FailureAsync(
                "SuperuserInvariantViolation",
                "transfer-superuser",
                target.Id,
                actorUserId
            );
        }

        var current = currentSuperusers.SingleOrDefault(user => user.Id == actorUserId);
        if (current is null)
        {
            return await FailureAsync(
                "CurrentSuperuserNotFound",
                "transfer-superuser",
                target.Id,
                actorUserId
            );
        }

        if (current.Id == target.Id)
        {
            return SuperuserOperationResult.Success(await ToResponseAsync(target, true));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var removeResult = await userManager.RemoveFromRoleAsync(
            current,
            SuperuserConstants.RoleName
        );
        var addResult = removeResult.Succeeded
            ? await userManager.AddToRoleAsync(target, SuperuserConstants.RoleName)
            : removeResult;

        if (!removeResult.Succeeded || !addResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return await FailureAsync(
                "SuperuserTransferFailed",
                "transfer-superuser",
                target.Id,
                actorUserId
            );
        }

        await userManager.UpdateSecurityStampAsync(current);
        await userManager.UpdateSecurityStampAsync(target);
        await transaction.CommitAsync(cancellationToken);
        await AuditAsync(
            actorUserId,
            target.Id,
            "transfer-superuser",
            true,
            null,
            cancellationToken
        );
        return SuperuserOperationResult.Success(await ToResponseAsync(target, true));
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> ResetSuperuserFromCommandAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureRoleAsync(cancellationToken);
        var existingSuperusers = await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName);
        if (existingSuperusers.Count != 1)
        {
            return await FailureAsync(
                "SuperuserInvariantViolation",
                "offline-reset-superuser",
                null,
                null
            );
        }

        var target = await userManager.FindByNameAsync(username);
        if (target is null)
        {
            return await FailureAsync("UserNotFound", "offline-reset-superuser", null, null);
        }

        if (!await userManager.IsInRoleAsync(target, SuperuserConstants.RoleName))
        {
            return await FailureAsync(
                "UserIsNotSuperuser",
                "offline-reset-superuser",
                target.Id,
                null
            );
        }

        return await ResetPasswordCoreAsync(
            target,
            "offline-reset-superuser",
            null,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<SuperuserOperationResult> BootstrapSuperuserFromCommandAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureRoleAsync(cancellationToken);
        var existingSuperusers = await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName);
        if (existingSuperusers.Count > 1)
        {
            return await FailureAsync(
                "SuperuserInvariantViolation",
                "offline-bootstrap-superuser",
                null,
                null
            );
        }

        if (existingSuperusers.Count == 1)
        {
            return await FailureAsync(
                "SuperuserAlreadyExists",
                "offline-bootstrap-superuser",
                null,
                null
            );
        }

        var target = await userManager.FindByNameAsync(username);
        if (target is null)
        {
            return await FailureAsync("UserNotFound", "offline-bootstrap-superuser", null, null);
        }

        if (target.IsDisabled)
        {
            return await FailureAsync(
                "DisabledUserCannotBeSuperuser",
                "offline-bootstrap-superuser",
                target.Id,
                null
            );
        }

        var result = await userManager.AddToRoleAsync(target, SuperuserConstants.RoleName);
        if (!result.Succeeded)
        {
            return await FailureAsync(
                "SuperuserBootstrapFailed",
                "offline-bootstrap-superuser",
                target.Id,
                null
            );
        }

        await userManager.UpdateSecurityStampAsync(target);
        await AuditAsync(
            null,
            target.Id,
            "offline-bootstrap-superuser",
            true,
            null,
            cancellationToken
        );
        return SuperuserOperationResult.Success(await ToResponseAsync(target, true));
    }

    private async Task<SuperuserOperationResult> ResetPasswordCoreAsync(
        ApplicationUser target,
        string action,
        Guid? actorUserId,
        CancellationToken cancellationToken
    )
    {
        var temporaryPassword = GenerateTemporaryPassword();
        var token = await userManager.GeneratePasswordResetTokenAsync(target);
        var resetResult = await userManager.ResetPasswordAsync(target, token, temporaryPassword);
        if (!resetResult.Succeeded)
        {
            return await FailureAsync("PasswordResetFailed", action, target.Id, actorUserId);
        }

        var logins = await userManager.GetLoginsAsync(target);
        if (!logins.Any(login => login.LoginProvider == SuperuserConstants.LocalLoginProvider))
        {
            var addLoginResult = await userManager.AddLoginAsync(
                target,
                new UserLoginInfo(
                    SuperuserConstants.LocalLoginProvider,
                    target.Id.ToString(),
                    localizer["LocalAccountProviderName"].Value
                )
            );
            if (!addLoginResult.Succeeded)
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer["ManagedUserLocalLoginMetadataWarningLog", target.Id]
                );
            }
        }

        target.MustChangePassword = true;
        var updateResult = await userManager.UpdateAsync(target);
        if (!updateResult.Succeeded)
        {
            return await FailureAsync("PasswordResetFailed", action, target.Id, actorUserId);
        }

        await userManager.UpdateSecurityStampAsync(target);
        await AuditAsync(actorUserId, target.Id, action, true, null, cancellationToken);
        return SuperuserOperationResult.Success(
            await ToResponseAsync(
                target,
                await userManager.IsInRoleAsync(target, SuperuserConstants.RoleName)
            ),
            temporaryPassword
        );
    }

    private async Task<ManagedUserResponse> ToResponseAsync(ApplicationUser user, bool isSuperuser)
    {
        var logins = await userManager.GetLoginsAsync(user);
        return new ManagedUserResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            await userManager.HasPasswordAsync(user),
            logins.Any(login => login.LoginProvider == "oidc"),
            user.IsDisabled,
            user.MustChangePassword,
            isSuperuser
        );
    }

    private async Task<SuperuserOperationResult> FailureAsync(
        string errorCode,
        string action,
        Guid? targetUserId,
        Guid? actorUserId
    )
    {
        await AuditAsync(actorUserId, targetUserId, action, false, errorCode, default);
        return SuperuserOperationResult.Failure(errorCode);
    }

    private async Task AuditAsync(
        Guid? actorUserId,
        Guid? targetUserId,
        string action,
        bool succeeded,
        string? details,
        CancellationToken cancellationToken
    )
    {
        dbContext.AuthenticationAdminAuditEvents.Add(
            new AuthenticationAdminAuditEvent
            {
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                Action = action,
                Succeeded = succeeded,
                CreatedAt = DateTimeOffset.UtcNow,
                RequestIpAddress =
                    httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                Details = details,
            }
        );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateTemporaryPassword()
    {
        var password = Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(TemporaryPasswordByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return password;
    }
}
