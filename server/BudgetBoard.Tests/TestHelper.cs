using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;

namespace BudgetBoard.IntegrationTests;

internal class TestHelper
{
    public readonly UserDataContext UserDataContext;
    public readonly ApplicationUser demoUser = _applicationUserFaker.Generate();

    private static readonly ApplicationUserFaker _applicationUserFaker = new();

    public TestHelper()
    {
        var builder = new DbContextOptionsBuilder<UserDataContext>();
        builder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new StringSanitizationInterceptor());

        var dbContextOptions = builder.Options;
        UserDataContext = new UserDataContext(dbContextOptions);
        // Delete existing db before creating a new one
        UserDataContext.Database.EnsureDeleted();
        UserDataContext.Database.EnsureCreated();

        // Seed a demo user
        UserDataContext.Users.Add(demoUser);
        UserDataContext.SaveChanges();
    }

    public static IStringLocalizer<T> CreateMockLocalizer<T>()
    {
        var mock = new Mock<IStringLocalizer<T>>();
        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns(
                (string key, object[] args) => new LocalizedString(key, string.Format(key, args))
            );
        return mock.Object;
    }
}
