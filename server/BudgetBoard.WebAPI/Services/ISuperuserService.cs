using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Models;

namespace BudgetBoard.WebAPI.Services;

public interface ISuperuserService
{
    /// <summary>
    /// Executes an operation once as part of first-user bootstrap, serializing concurrent calls on
    /// PostgreSQL without retrying the operation.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The result returned by <paramref name="operation"/>.</returns>
    Task<TResult> ExecuteFirstUserBootstrapAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a user identity mutation while serialized with user bootstrap on PostgreSQL.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The result returned by <paramref name="operation"/>.</returns>
    Task<TResult> ExecuteWithUserIdentityLockAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Ensures that the superuser role exists.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureRoleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that the superuser role exists and promotes an eligible existing account if no superuser exists.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureRoleAndBootstrapAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Promotes the specified user when they are the only user and no superuser exists.
    /// </summary>
    /// <param name="user">The user to promote if they are the first user.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureFirstUserSuperuserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the user accounts and their superuser-management status.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The managed-user details for all accounts.</returns>
    Task<IReadOnlyList<ManagedUserResponse>> ListUsersAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a filtered, paginated list of authentication-management audit events.
    /// </summary>
    /// <param name="query">The filters and paging options to apply.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The matching events and available action filters.</returns>
    Task<AdminAuditEventsPageResponse> ListAuditEventsAsync(
        AdminAuditEventsQuery query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates a user account that must change its generated temporary password at the next login.
    /// </summary>
    /// <param name="request">The username and optional email address for the new account.</param>
    /// <param name="actorUserId">The ID of the user performing the operation, for auditing.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the created user and temporary password on success.</returns>
    Task<SuperuserOperationResult> CreateUserAsync(
        CreateManagedUserRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes the username of the acting superuser or a non-superuser account.
    /// </summary>
    /// <param name="targetUserId">The ID of the account whose username to change.</param>
    /// <param name="request">The new username.</param>
    /// <param name="actorUserId">The ID of the user performing the operation, for auditing.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the updated user on success.</returns>
    Task<SuperuserOperationResult> UpdateUsernameAsync(
        Guid targetUserId,
        UpdateManagedUserUsernameRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resets the specified user's password and generates a temporary password.
    /// </summary>
    /// <param name="targetUserId">The ID of the user whose password to reset.</param>
    /// <param name="actorUserId">The ID of the user performing the operation, for auditing.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the updated user and temporary password on success.</returns>
    Task<SuperuserOperationResult> ResetPasswordAsync(
        Guid targetUserId,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enables or disables the specified user account.
    /// </summary>
    /// <param name="targetUserId">The ID of the account to update.</param>
    /// <param name="actorUserId">The ID of the user performing the operation, for auditing.</param>
    /// <param name="disabled"><see langword="true"/> to disable the account; otherwise, enable it.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the updated user on success.</returns>
    Task<SuperuserOperationResult> SetDisabledAsync(
        Guid targetUserId,
        Guid actorUserId,
        bool disabled,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Transfers the superuser role from the acting superuser to the specified user.
    /// </summary>
    /// <param name="targetUserId">The ID of the user who will receive the superuser role.</param>
    /// <param name="actorUserId">The ID of the current superuser transferring the role.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the target user on success.</returns>
    Task<SuperuserOperationResult> TransferSuperuserAsync(
        Guid targetUserId,
        Guid actorUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resets the password of an existing superuser from an offline command.
    /// </summary>
    /// <param name="username">The username of the superuser whose password to reset.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the updated user and temporary password on success.</returns>
    Task<SuperuserOperationResult> ResetSuperuserFromCommandAsync(
        string username,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Promotes an existing enabled user to superuser when no superuser currently exists.
    /// </summary>
    /// <param name="username">The username of the user to promote.</param>
    /// <param name="cancellationToken">A token that can cancel the database operations.</param>
    /// <returns>The operation result, including the promoted user on success.</returns>
    Task<SuperuserOperationResult> BootstrapSuperuserFromCommandAsync(
        string username,
        CancellationToken cancellationToken = default
    );
}
