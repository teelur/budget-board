using BudgetBoard.WebAPI.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
}
