using System.Security.Claims;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Middleware;
using BudgetBoard.WebAPI.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BudgetBoard.IntegrationTests;

public sealed class MustChangePasswordMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_BlocksFinancialRoutesUntilPasswordChanges()
    {
        await using var provider = CreateProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "temporary", MustChangePassword = true };
        (await userManager.CreateAsync(user, "password")).Succeeded.Should().BeTrue();

        var context = CreateContext(provider, user.Id, "/api/accounts", HttpMethods.Get);
        var nextCalled = false;
        var middleware = new MustChangePasswordMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, provider.GetRequiredService<IStringLocalizer<ApiResponseStrings>>());

        await middleware.InvokeAsync(
            context,
            provider.GetRequiredService<UserManager<ApplicationUser>>()
        );

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_AllowsTheExistingPasswordChangeSurface()
    {
        await using var provider = CreateProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "temporary", MustChangePassword = true };
        (await userManager.CreateAsync(user, "password")).Succeeded.Should().BeTrue();

        var context = CreateContext(provider, user.Id, "/api/manage/info", HttpMethods.Post);
        var nextCalled = false;
        var middleware = new MustChangePasswordMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, provider.GetRequiredService<IStringLocalizer<ApiResponseStrings>>());

        await middleware.InvokeAsync(
            context,
            provider.GetRequiredService<UserManager<ApplicationUser>>()
        );

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AllowsTheAccountCapabilityRead()
    {
        await using var provider = CreateProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "temporary", MustChangePassword = true };
        (await userManager.CreateAsync(user, "password")).Succeeded.Should().BeTrue();

        var context = CreateContext(provider, user.Id, "/api/applicationUser", HttpMethods.Get);
        var nextCalled = false;
        var middleware = new MustChangePasswordMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, provider.GetRequiredService<IStringLocalizer<ApiResponseStrings>>());

        await middleware.InvokeAsync(
            context,
            provider.GetRequiredService<UserManager<ApplicationUser>>()
        );

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UsesUserLoadedDuringAuthentication()
    {
        await using var provider = CreateProvider();
        var userManager = CreateUserManagerMock();
        var user = new ApplicationUser { UserName = "temporary", MustChangePassword = true };
        var context = CreateContext(provider, user.Id, "/api/accounts", HttpMethods.Get);
        context.Items[typeof(ApplicationUser)] = user;
        var nextCalled = false;
        var middleware = new MustChangePasswordMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, provider.GetRequiredService<IStringLocalizer<ApiResponseStrings>>());

        await middleware.InvokeAsync(context, userManager.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        nextCalled.Should().BeFalse();
        userManager.Verify(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    private static DefaultHttpContext CreateContext(
        IServiceProvider provider,
        Guid userId,
        string path,
        string method
    )
    {
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = path;
        context.Request.Method = method;
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "test"
            )
        );
        return context;
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        services.AddLocalization();
        services.AddDbContext<UserDataContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
        );
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<UserDataContext>();
        return services.BuildServiceProvider();
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock() =>
        new(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance
        );
}
