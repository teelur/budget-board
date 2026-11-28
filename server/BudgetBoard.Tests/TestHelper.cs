using BudgetBoard.Database.Data;
using BudgetBoard.Database.Helpers;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service.Interfaces;
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
                (string key, object[] args) =>
                    new LocalizedString(key, $"{key} [{string.Join(", ", args)}]")
            );
        return mock.Object;
    }

    public static INotificationActivityService CreateMockNotificationActivityService()
    {
        var mock = new Mock<INotificationActivityService>();
        mock.Setup(service => service.BeginAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new NoopNotificationActivityScope());
        mock.Setup(service =>
                service.TrackTransactionCreatedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Transaction>()
                )
            )
            .Returns(Task.CompletedTask);
        mock.Setup(service =>
                service.TrackBudgetMonthAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>())
            )
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private sealed class NoopNotificationActivityScope : INotificationActivityScope
    {
        public Task CompleteAsync() => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
