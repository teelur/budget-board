using System.Net.Sockets;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Npgsql;

namespace BudgetBoard.IntegrationTests;

public sealed class SuperuserServiceTests
{
    [Fact]
    public async Task OptionalEmailValidator_AllowsUsernameOnlyAccountsAndPreservesUniqueness()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();

        var usernameOnlyResult = await userManager.CreateAsync(
            new ApplicationUser { UserName = "username-only@example.test" },
            "password"
        );
        usernameOnlyResult.Succeeded.Should().BeTrue();
        (
            await userManager.FindByNameAsync("username-only@example.test")
        )!.EmailConfirmed.Should().BeFalse();

        var firstEmailUser = new ApplicationUser
        {
            UserName = "first-email-user",
            Email = "shared@example.test",
        };
        (await userManager.CreateAsync(firstEmailUser, "password")).Succeeded.Should().BeTrue();

        var duplicateEmailResult = await userManager.CreateAsync(
            new ApplicationUser
            {
                UserName = "duplicate-email-user",
                Email = "shared@example.test",
            },
            "password"
        );
        duplicateEmailResult.Succeeded.Should().BeFalse();
        duplicateEmailResult.Errors.Should().Contain(error => error.Code == "DuplicateEmail");

        var usernameEmailCollisionResult = await userManager.CreateAsync(
            new ApplicationUser { UserName = "shared@example.test" },
            "password"
        );
        usernameEmailCollisionResult.Succeeded.Should().BeFalse();
        usernameEmailCollisionResult.Errors.Should().Contain(error =>
            error.Code == "DuplicateUserName"
        );

        var emailUsernameCollisionResult = await userManager.CreateAsync(
            new ApplicationUser
            {
                UserName = "email-username-collision",
                Email = "username-only@example.test",
            },
            "password"
        );
        emailUsernameCollisionResult.Succeeded.Should().BeFalse();
        emailUsernameCollisionResult.Errors.Should().Contain(error =>
            error.Code == "DuplicateEmail"
        );
    }

    [Fact]
    public async Task ListAuditEventsAsync_FiltersByActionResultAndUserAndPaginatesNewestFirst()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var harness = CreateHarness(connection);
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        var target = new ApplicationUser { UserName = "managed-user" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(target, "password")).Succeeded.Should().BeTrue();

        var now = DateTimeOffset.UtcNow;
        var oldestEvent = new AuthenticationAdminAuditEvent
        {
            Id = 3,
            ActorUserId = actor.Id,
            TargetUserId = target.Id,
            Action = "create-user",
            Succeeded = true,
            CreatedAt = now.AddMinutes(-2),
        };
        var failedEvent = new AuthenticationAdminAuditEvent
        {
            Id = 2,
            ActorUserId = actor.Id,
            TargetUserId = target.Id,
            Action = "disable-user",
            Succeeded = false,
            CreatedAt = now.AddMinutes(-1),
            Details = "SuperuserDisableNotAllowed",
        };
        var newestEvent = new AuthenticationAdminAuditEvent
        {
            Id = 1,
            ActorUserId = actor.Id,
            TargetUserId = target.Id,
            Action = "reset-password",
            Succeeded = true,
            CreatedAt = now,
        };
        dbContext.AuthenticationAdminAuditEvents.AddRange(
            oldestEvent,
            failedEvent,
            newestEvent
        );
        await dbContext.SaveChangesAsync();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        var filtered = await service.ListAuditEventsAsync(
            new AdminAuditEventsQuery
            {
                Action = "disable-user",
                Succeeded = false,
                UserName = "managed",
            }
        );

        filtered.TotalCount.Should().Be(1);
        filtered.Events.Should().ContainSingle();
        filtered.Events[0].Id.Should().Be(failedEvent.Id);
        filtered.Events[0].ActorUserName.Should().Be(actor.UserName);
        filtered.Events[0].TargetUserName.Should().Be(target.UserName);
        filtered.Events[0].Details.Should().Be("SuperuserDisableNotAllowed");
        filtered.Actions.Should().Contain(["create-user", "disable-user", "reset-password"]);

        var secondPage = await service.ListAuditEventsAsync(
            new AdminAuditEventsQuery { Page = 2, PageSize = 2 }
        );

        secondPage.TotalCount.Should().Be(3);
        secondPage.Events.Should().ContainSingle();
        secondPage.Events[0].Id.Should().Be(oldestEvent.Id);
    }

    [Fact]
    public async Task EnsureRoleAndBootstrapAsync_PromotesTheOnlyExistingUser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(user, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();

        (await userManager.IsInRoleAsync(user, SuperuserConstants.RoleName)).Should().BeTrue();
    }

    [Fact]
    public async Task EnsureRoleAndBootstrapAsync_RejectsADisabledOnlyUser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "disabled", IsDisabled = true };
        (await userManager.CreateAsync(user, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EnsureRoleAndBootstrapAsync()
        );

        (await userManager.IsInRoleAsync(user, SuperuserConstants.RoleName)).Should().BeFalse();
    }

    [Fact]
    public async Task EnsureRoleAndBootstrapAsync_RejectsAConfiguredDisabledUser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var enabledUser = new ApplicationUser { UserName = "enabled" };
        var disabledUser = new ApplicationUser { UserName = "disabled", IsDisabled = true };
        (await userManager.CreateAsync(enabledUser, "password")).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(disabledUser, "password")).Succeeded.Should().BeTrue();
        harness.Services.GetRequiredService<IConfiguration>()[
            SuperuserConstants.BootstrapUsernameConfigurationKey
        ] = disabledUser.UserName;

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EnsureRoleAndBootstrapAsync()
        );

        (await userManager.IsInRoleAsync(enabledUser, SuperuserConstants.RoleName)).Should().BeFalse();
        (await userManager.IsInRoleAsync(disabledUser, SuperuserConstants.RoleName)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateUserAsync_GeneratesAForcedChangePassword()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var result = await service.CreateUserAsync(
            new CreateManagedUserRequest { Username = "managed" },
            actor.Id
        );

        result.Succeeded.Should().BeTrue();
        result.TemporaryPassword.Should().NotBeNullOrWhiteSpace();
        var managed = await userManager.FindByNameAsync("managed");
        managed.Should().NotBeNull();
        managed!.MustChangePassword.Should().BeTrue();
        (await userManager.CheckPasswordAsync(managed, result.TemporaryPassword!)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_RejectsUsernameEmailIdentifierCollisions()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        (await userManager.CreateAsync(
            new ApplicationUser { UserName = "email-owner", Email = "reserved@example.test" },
            "password"
        )).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(
            new ApplicationUser { UserName = "username@example.test" },
            "password"
        )).Succeeded.Should().BeTrue();

        var usernameCollision = await service.CreateUserAsync(
            new CreateManagedUserRequest { Username = "reserved@example.test" },
            actor.Id
        );
        usernameCollision.Succeeded.Should().BeFalse();
        usernameCollision.ErrorCode.Should().Be("UsernameAlreadyTaken");

        var emailCollision = await service.CreateUserAsync(
            new CreateManagedUserRequest
            {
                Username = "new-email-user",
                Email = "username@example.test",
            },
            actor.Id
        );
        emailCollision.Succeeded.Should().BeFalse();
        emailCollision.ErrorCode.Should().Be("EmailAlreadyTaken");
    }

    [Fact]
    public async Task UpdateUsernameAsync_UpdatesNonSuperuserUsernameAndAuditsTheChange()
    {
        await using var harness = CreateHarness();
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        var target = new ApplicationUser { UserName = "managed" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();
        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        (await userManager.CreateAsync(target, "password")).Succeeded.Should().BeTrue();

        var result = await service.UpdateUsernameAsync(
            target.Id,
            new UpdateManagedUserUsernameRequest { Username = "renamed-managed" },
            actor.Id
        );

        result.Succeeded.Should().BeTrue();
        result.User!.Username.Should().Be("renamed-managed");
        (await userManager.FindByNameAsync("managed")).Should().BeNull();
        (await userManager.FindByNameAsync("renamed-managed"))!.Id.Should().Be(target.Id);
        (
            await dbContext.AuthenticationAdminAuditEvents
                .Where(audit =>
                    audit.TargetUserId == target.Id && audit.Action == "update-username"
                )
                .ToListAsync()
        ).Should().ContainSingle(audit => audit.Succeeded);
    }

    [Fact]
    public async Task UpdateUsernameAsync_RejectsDuplicateUsernames()
    {
        await using var harness = CreateHarness();
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        var target = new ApplicationUser { UserName = "managed" };
        var existing = new ApplicationUser { UserName = "existing" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();
        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        (await userManager.CreateAsync(target, "password")).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(existing, "password")).Succeeded.Should().BeTrue();

        var result = await service.UpdateUsernameAsync(
            target.Id,
            new UpdateManagedUserUsernameRequest { Username = "EXISTING" },
            actor.Id
        );

        result.Succeeded.Should().BeFalse();
        result.ErrorCode.Should().Be("UsernameAlreadyTaken");
        dbContext.ChangeTracker.Clear();
        (await userManager.FindByIdAsync(target.Id.ToString()))!.UserName.Should().Be("managed");
        (
            await dbContext.AuthenticationAdminAuditEvents
                .Where(audit =>
                    audit.TargetUserId == target.Id && audit.Action == "update-username"
                )
                .ToListAsync()
        ).Should().ContainSingle(audit => !audit.Succeeded);
    }

    [Fact]
    public async Task UpdateUsernameAsync_AllowsSelfButNotAnotherSuperuser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        var otherSuperuser = new ApplicationUser { UserName = "other-owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();
        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        (await userManager.CreateAsync(otherSuperuser, "password")).Succeeded.Should().BeTrue();
        (
            await userManager.AddToRoleAsync(otherSuperuser, SuperuserConstants.RoleName)
        ).Succeeded.Should().BeTrue();

        var rejectedResult = await service.UpdateUsernameAsync(
            otherSuperuser.Id,
            new UpdateManagedUserUsernameRequest { Username = "renamed-other-owner" },
            actor.Id
        );

        rejectedResult.Succeeded.Should().BeFalse();
        rejectedResult.ErrorCode.Should().Be("SuperuserUsernameChangeNotAllowed");
        (await userManager.FindByNameAsync("other-owner"))!.Id.Should().Be(otherSuperuser.Id);

        var selfResult = await service.UpdateUsernameAsync(
            actor.Id,
            new UpdateManagedUserUsernameRequest { Username = "renamed-owner" },
            actor.Id
        );

        selfResult.Succeeded.Should().BeTrue();
        selfResult.User!.IsSuperuser.Should().BeTrue();
        (await userManager.FindByNameAsync("renamed-owner"))!.Id.Should().Be(actor.Id);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidatesTheOldPasswordAndRequiresChange()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var result = await service.ResetPasswordAsync(actor.Id, actor.Id);

        result.Succeeded.Should().BeTrue();
        result.TemporaryPassword.Should().NotBeNullOrWhiteSpace();
        (await userManager.CheckPasswordAsync(actor, "password")).Should().BeFalse();
        (await userManager.CheckPasswordAsync(actor, result.TemporaryPassword!)).Should().BeTrue();
        actor.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenPasswordChangeFlagUpdateFails_RollsBackTheReset()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var harness = CreateHarness(
            connection,
            new RejectPasswordChangeValidator()
        );
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var result = await service.ResetPasswordAsync(actor.Id, actor.Id);

        result.Succeeded.Should().BeFalse();
        result.TemporaryPassword.Should().BeNull();
        dbContext.ChangeTracker.Clear();
        var persistedActor = (await userManager.FindByIdAsync(actor.Id.ToString()))!;
        (await userManager.CheckPasswordAsync(persistedActor, "password")).Should().BeTrue();
        persistedActor.MustChangePassword.Should().BeFalse();
        (
            await userManager.GetLoginsAsync(persistedActor)
        ).Should()
            .NotContain(login =>
                login.LoginProvider == SuperuserConstants.LocalLoginProvider
            );
        (
            await dbContext.AuthenticationAdminAuditEvents
                .Where(audit =>
                    audit.TargetUserId == actor.Id && audit.Action == "reset-password"
                )
                .ToListAsync()
        ).Should().ContainSingle(audit => !audit.Succeeded);
    }

    [Fact]
    public async Task ResetPasswordAsync_AddsALocalCredentialToAnOidcOnlyUser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();
        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();

        var oidcOnlyUser = new ApplicationUser
        {
            UserName = "oidc-user",
            Email = "oidc@example.test",
            EmailConfirmed = true,
        };
        (await userManager.CreateAsync(oidcOnlyUser)).Succeeded.Should().BeTrue();

        var result = await service.ResetPasswordAsync(oidcOnlyUser.Id, actor.Id);

        result.Succeeded.Should().BeTrue();
        (
            await userManager.GetLoginsAsync(
                (await userManager.FindByIdAsync(oidcOnlyUser.Id.ToString()))!
            )
        ).Should().ContainSingle(login => login.LoginProvider == SuperuserConstants.LocalLoginProvider);
    }

    [Fact]
    public async Task SetDisabledAsync_ChangesTheAccountState()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var createResult = await service.CreateUserAsync(
            new CreateManagedUserRequest { Username = "managed" },
            actor.Id
        );
        var managedId = createResult.User!.Id;

        var disableResult = await service.SetDisabledAsync(managedId, actor.Id, true);
        disableResult.Succeeded.Should().BeTrue();
        (await userManager.FindByIdAsync(managedId.ToString()))!.IsDisabled.Should().BeTrue();

        var enableResult = await service.SetDisabledAsync(managedId, actor.Id, false);
        enableResult.Succeeded.Should().BeTrue();
        (await userManager.FindByIdAsync(managedId.ToString()))!.IsDisabled.Should().BeFalse();
    }

    [Fact]
    public async Task AdminMutations_WhenAuditWriteFails_RollBackTheirChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var harness = CreateHarness(connection);
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var createResult = await service.CreateUserAsync(
            new CreateManagedUserRequest { Username = "managed" },
            actor.Id
        );
        var managedId = createResult.User!.Id;

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TRIGGER reject_admin_audit_insert
            BEFORE INSERT ON "AuthenticationAdminAuditEvent"
            BEGIN
                SELECT RAISE(ABORT, 'audit insert rejected');
            END;
            """
        );

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.SetDisabledAsync(managedId, actor.Id, true)
        );

        dbContext.ChangeTracker.Clear();
        (await userManager.FindByIdAsync(managedId.ToString()))!.IsDisabled.Should().BeFalse();
        (
            await dbContext.AuthenticationAdminAuditEvents
                .Where(audit =>
                    audit.TargetUserId == managedId && audit.Action == "disable-user"
                )
                .ToListAsync()
        ).Should().BeEmpty();

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.CreateUserAsync(
                new CreateManagedUserRequest { Username = "audit-failed" },
                actor.Id
            )
        );

        dbContext.ChangeTracker.Clear();
        (await userManager.FindByNameAsync("audit-failed")).Should().BeNull();
    }

    [Fact]
    public async Task TransferSuperuserAsync_LeavesExactlyOneSuperuser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var createResult = await service.CreateUserAsync(
            new CreateManagedUserRequest { Username = "managed" },
            actor.Id
        );

        var transferResult = await service.TransferSuperuserAsync(
            createResult.User!.Id,
            actor.Id
        );

        transferResult.Succeeded.Should().BeTrue();
        (await userManager.IsInRoleAsync(actor, SuperuserConstants.RoleName)).Should().BeFalse();
        (await userManager.IsInRoleAsync(
            (await userManager.FindByNameAsync("managed"))!,
            SuperuserConstants.RoleName
        )).Should().BeTrue();
        (await userManager.GetUsersInRoleAsync(SuperuserConstants.RoleName)).Should().ContainSingle();
    }

    [Fact]
    public async Task TransferSuperuserAsync_WhenSecurityStampUpdateFails_RollsBackTransfer()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var validator = new RejectUserUpdateValidator();
        await using var harness = CreateHarness(connection, validator);
        var dbContext = harness.Services.GetRequiredService<UserDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var actor = new ApplicationUser { UserName = "owner" };
        (await userManager.CreateAsync(actor, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        await service.EnsureRoleAndBootstrapAsync();
        var target = new ApplicationUser { UserName = "target" };
        (await userManager.CreateAsync(target, "password")).Succeeded.Should().BeTrue();
        var actorSecurityStamp = await userManager.GetSecurityStampAsync(actor);
        var targetSecurityStamp = await userManager.GetSecurityStampAsync(target);

        validator.RejectedUsername = actor.UserName;
        validator.OriginalSecurityStamp = actorSecurityStamp;
        var currentStampFailure = await service.TransferSuperuserAsync(target.Id, actor.Id);

        currentStampFailure.Succeeded.Should().BeFalse();
        currentStampFailure.ErrorCode.Should().Be("SuperuserTransferFailed");
        dbContext.ChangeTracker.Clear();
        var persistedActor = (await userManager.FindByIdAsync(actor.Id.ToString()))!;
        var persistedTarget = (await userManager.FindByIdAsync(target.Id.ToString()))!;
        (await userManager.IsInRoleAsync(persistedActor, SuperuserConstants.RoleName)).Should()
            .BeTrue();
        (await userManager.IsInRoleAsync(persistedTarget, SuperuserConstants.RoleName)).Should()
            .BeFalse();
        (await userManager.GetSecurityStampAsync(persistedActor)).Should().Be(actorSecurityStamp);

        validator.RejectedUsername = target.UserName;
        validator.OriginalSecurityStamp = targetSecurityStamp;
        var targetStampFailure = await service.TransferSuperuserAsync(target.Id, actor.Id);

        targetStampFailure.Succeeded.Should().BeFalse();
        targetStampFailure.ErrorCode.Should().Be("SuperuserTransferFailed");
        dbContext.ChangeTracker.Clear();
        persistedActor = (await userManager.FindByIdAsync(actor.Id.ToString()))!;
        persistedTarget = (await userManager.FindByIdAsync(target.Id.ToString()))!;
        (await userManager.IsInRoleAsync(persistedActor, SuperuserConstants.RoleName)).Should()
            .BeTrue();
        (await userManager.IsInRoleAsync(persistedTarget, SuperuserConstants.RoleName)).Should()
            .BeFalse();
        (await userManager.GetSecurityStampAsync(persistedActor)).Should().Be(actorSecurityStamp);
        (await userManager.GetSecurityStampAsync(persistedTarget)).Should().Be(targetSecurityStamp);
        (
            await dbContext.AuthenticationAdminAuditEvents
                .Where(audit =>
                    audit.TargetUserId == target.Id && audit.Action == "transfer-superuser"
                )
                .ToListAsync()
        ).Should()
            .HaveCount(2)
            .And.OnlyContain(audit => !audit.Succeeded);
    }

    [Fact]
    public async Task BootstrapSuperuserFromCommandAsync_PromotesTheNamedExistingUser()
    {
        await using var harness = CreateHarness();
        var userManager = harness.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var first = new ApplicationUser { UserName = "first" };
        var selected = new ApplicationUser { UserName = "selected" };
        (await userManager.CreateAsync(first, "password")).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(selected, "password")).Succeeded.Should().BeTrue();

        var service = harness.Services.GetRequiredService<ISuperuserService>();
        var result = await service.BootstrapSuperuserFromCommandAsync("selected");

        result.Succeeded.Should().BeTrue();
        (await userManager.IsInRoleAsync(selected, SuperuserConstants.RoleName)).Should().BeTrue();
        (await userManager.IsInRoleAsync(first, SuperuserConstants.RoleName)).Should().BeFalse();
    }

    [PostgreSqlFact]
    public async Task ExecuteFirstUserBootstrapAsync_ExecutesQueriesWithoutRetryingTransaction()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "BUDGET_BOARD_TEST_POSTGRES_CONNECTION_STRING"
        );

        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
            )
            .Options;
        await using var dbContext = new UserDataContext(options);
        var service = new SuperuserService(
            null!,
            null!,
            dbContext,
            null!,
            null!,
            null!,
            NullLogger<SuperuserService>.Instance,
            null!,
            null!
        );

        var operationAttempts = 0;
        var transientException = new NpgsqlException(
            "Simulated transient network failure.",
            new SocketException((int)SocketError.ConnectionReset)
        );
        transientException.IsTransient.Should().BeTrue();
        var exception = await Assert.ThrowsAsync<NpgsqlException>(() =>
            service.ExecuteFirstUserBootstrapAsync<int>(async () =>
            {
                operationAttempts++;
                await dbContext.Database.SqlQuery<int>($"SELECT 1 AS \"Value\"").SingleAsync();
                throw transientException;
            })
        );

        exception.Should().BeSameAs(transientException);
        operationAttempts.Should().Be(1);

        var operationRan = false;
        var result = await service.ExecuteWithUserIdentityLockAsync(async () =>
        {
            operationRan = true;
            return await dbContext.Database.SqlQuery<int>($"SELECT 1 AS \"Value\"").SingleAsync();
        });

        result.Should().Be(1);
        operationRan.Should().BeTrue();
    }

    private static ServiceProviderHarness CreateHarness(
        SqliteConnection? sqliteConnection = null,
        IUserValidator<ApplicationUser>? additionalUserValidator = null
    )
    {
        var services = new ServiceCollection();
        var widgetSettingsService = new Mock<IWidgetSettingsService>();
        services.AddSingleton(widgetSettingsService.Object);
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddLocalization();
        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder().AddInMemoryCollection().Build()
        );
        services.AddDbContext<UserDataContext>(options =>
        {
            if (sqliteConnection is null)
            {
                options
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)
                    );
            }
            else
            {
                options.UseSqlite(sqliteConnection);
            }
        });
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<UserDataContext>()
            .AddUserValidator<OptionalEmailUserValidator>()
            .AddDefaultTokenProviders();
        if (additionalUserValidator is not null)
        {
            services.AddSingleton(additionalUserValidator);
        }
        services.AddScoped<ISuperuserService, SuperuserService>();
        return new ServiceProviderHarness(services.BuildServiceProvider());
    }

    private sealed class RejectPasswordChangeValidator : IUserValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user
        ) =>
            Task.FromResult(
                user.MustChangePassword
                    ? IdentityResult.Failed(
                        new IdentityError { Code = "PasswordChangeNotAllowed" }
                    )
                    : IdentityResult.Success
            );
    }

    private sealed class RejectUserUpdateValidator : IUserValidator<ApplicationUser>
    {
        public string? RejectedUsername { get; set; }
        public string? OriginalSecurityStamp { get; set; }

        public Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user
        ) =>
            Task.FromResult(
                user.UserName == RejectedUsername && user.SecurityStamp != OriginalSecurityStamp
                    ? IdentityResult.Failed(new IdentityError { Code = "UserUpdateRejected" })
                    : IdentityResult.Success
            );
    }

    private sealed class ServiceProviderHarness : IAsyncDisposable
    {
        public ServiceProvider Services { get; }

        public ServiceProviderHarness(ServiceProvider services)
        {
            Services = services;
        }

        public async ValueTask DisposeAsync()
        {
            await Services.DisposeAsync();
        }
    }

    public sealed class PostgreSqlFactAttribute : FactAttribute
    {
        public PostgreSqlFactAttribute()
        {
            if (
                string.IsNullOrWhiteSpace(
                    Environment.GetEnvironmentVariable("BUDGET_BOARD_TEST_POSTGRES_CONNECTION_STRING")
                )
            )
            {
                Skip =
                    "Set BUDGET_BOARD_TEST_POSTGRES_CONNECTION_STRING to run PostgreSQL integration tests.";
            }
        }
    }
}
