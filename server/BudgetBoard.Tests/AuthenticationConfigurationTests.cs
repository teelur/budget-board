using System.Security.Claims;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class AuthenticationConfigurationTests
{
    [Fact]
    public async Task AddBudgetBoardAuthentication_RegistersIdentityCookieSchemes()
    {
        var services = new ServiceCollection();

        services.AddBudgetBoardAuthentication();
        using var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        var applicationScheme = await schemeProvider.GetSchemeAsync(
            IdentityConstants.ApplicationScheme
        );
        var twoFactorRememberMeScheme = await schemeProvider.GetSchemeAsync(
            IdentityConstants.TwoFactorRememberMeScheme
        );
        var twoFactorUserIdScheme = await schemeProvider.GetSchemeAsync(
            IdentityConstants.TwoFactorUserIdScheme
        );

        applicationScheme.Should().NotBeNull();
        twoFactorRememberMeScheme.Should().NotBeNull();
        twoFactorUserIdScheme.Should().NotBeNull();
    }

    [Fact]
    public async Task AddBudgetBoardAuthentication_CachesValidatedUserForTheRequest()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBudgetBoardAuthentication();
        var user = new ApplicationUser { UserName = "owner" };
        const string securityStamp = "valid-security-stamp";
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(
                        new IdentityOptions().ClaimsIdentity.SecurityStampClaimType,
                        securityStamp
                    ),
                ],
                IdentityConstants.ApplicationScheme
            )
        );
        var userManager = new Mock<UserManager<ApplicationUser>>(
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
        userManager.Setup(manager => manager.GetUserAsync(principal)).ReturnsAsync(user);
        userManager.Setup(manager => manager.GetSecurityStampAsync(user)).ReturnsAsync(securityStamp);
        services.AddSingleton(userManager.Object);

        using var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = (await schemeProvider.GetSchemeAsync(IdentityConstants.ApplicationScheme))!;
        var options = provider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
        var httpContext = new DefaultHttpContext { RequestServices = provider };
        var ticket = new AuthenticationTicket(principal, scheme.Name);
        var validationContext = new CookieValidatePrincipalContext(
            httpContext,
            scheme,
            options,
            ticket
        );

        await options.Events.OnValidatePrincipal(validationContext);

        httpContext.Items[typeof(ApplicationUser)].Should().BeSameAs(user);
    }
}
