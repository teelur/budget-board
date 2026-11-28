using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class RecurringReminderNotificationServiceTests
{
    [Fact]
    public async Task CheckAfterSyncAsync_WhenOccurrenceMatchesLeadTime_NotifiesOncePerLocalDay()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new UserDataContext(options);
        await context.Database.EnsureCreatedAsync();

        var user = new ApplicationUserFaker().Generate();
        const int leadTimeDays = 5;
        var today = new DateOnly(2026, 11, 29);
        context.Users.Add(user);
        context.UserSettings.Add(
            new UserSettings { UserID = user.Id, RecurringReminderLeadTimeDays = leadTimeDays }
        );
        await context.SaveChangesAsync();

        var recurringRuleService = new Mock<IRecurringRuleService>();
        recurringRuleService
            .Setup(service => service.ReadForecastAsync(user.Id, It.IsAny<DateOnly>()))
            .Returns<Guid, DateOnly>(
                (_, _) =>
                    Task.FromResult<IReadOnlyList<RecurringForecastOccurrenceResponse>>(
                        [
                            new RecurringForecastOccurrenceResponse
                            {
                                RuleID = Guid.NewGuid(),
                                Date = today.AddDays(leadTimeDays),
                                Amount = -25,
                                MerchantName = "Electric bill",
                            },
                        ]
                    )
            );
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service =>
                service.HasEnabledDestinationAsync(
                    user.Id,
                    NotificationEventTypes.RecurringReminder
                )
            )
            .ReturnsAsync(true);
        dispatcher
            .Setup(service => service.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(() => today);
        var service = CreateService(
            context,
            recurringRuleService.Object,
            dispatcher.Object,
            nowProvider.Object
        );

        await service.CheckAfterSyncAsync(user.Id);
        await service.CheckAfterSyncAsync(user.Id);
        today = today.AddDays(1);
        await service.CheckAfterSyncAsync(user.Id);

        dispatcher.Verify(
            item =>
                item.DispatchAsync(
                    It.Is<NotificationEvent>(notification =>
                        notification.UserId == user.Id
                        && notification.Type == NotificationEventTypes.RecurringReminder
                        && notification.FinancialDetails!.Contains("Electric bill")
                    )
                ),
            Times.Exactly(2)
        );
        var lastReminderDate = await context.ApplicationUsers
            .Where(applicationUser => applicationUser.Id == user.Id)
            .Select(applicationUser => applicationUser.LastRecurringReminderDate)
            .SingleAsync();
        lastReminderDate.Should().Be(today);
    }

    [Fact]
    public async Task CheckAfterSyncAsync_WhenNoOccurrenceMatches_DoesNotClaimTheDay()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new UserDataContext(options);
        await context.Database.EnsureCreatedAsync();

        var user = new ApplicationUserFaker().Generate();
        context.Users.Add(user);
        context.UserSettings.Add(new UserSettings { UserID = user.Id });
        await context.SaveChangesAsync();

        var recurringRuleService = new Mock<IRecurringRuleService>();
        recurringRuleService
            .Setup(service => service.ReadForecastAsync(user.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync((IReadOnlyList<RecurringForecastOccurrenceResponse>)[]);
        var dispatcher = new Mock<INotificationDispatcher>();
        var today = new DateOnly(2026, 11, 29);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var service = CreateService(
            context,
            recurringRuleService.Object,
            dispatcher.Object,
            nowProvider.Object
        );

        await service.CheckAfterSyncAsync(user.Id);

        var lastReminderDate = await context.ApplicationUsers
            .Where(applicationUser => applicationUser.Id == user.Id)
            .Select(applicationUser => applicationUser.LastRecurringReminderDate)
            .SingleAsync();
        lastReminderDate.Should().BeNull();
        dispatcher.Verify(
            item => item.DispatchAsync(It.IsAny<NotificationEvent>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CheckAfterSyncAsync_WhenNoDestinationSubscribes_DoesNotClaimTheDay()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new UserDataContext(options);
        await context.Database.EnsureCreatedAsync();

        var user = new ApplicationUserFaker().Generate();
        const int leadTimeDays = 5;
        var today = new DateOnly(2026, 11, 29);
        context.Users.Add(user);
        context.UserSettings.Add(
            new UserSettings { UserID = user.Id, RecurringReminderLeadTimeDays = leadTimeDays }
        );
        await context.SaveChangesAsync();

        var recurringRuleService = new Mock<IRecurringRuleService>();
        recurringRuleService
            .Setup(service => service.ReadForecastAsync(user.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(
                (IReadOnlyList<RecurringForecastOccurrenceResponse>)
                [
                    new()
                    {
                        RuleID = Guid.NewGuid(),
                        Date = today.AddDays(leadTimeDays),
                        Amount = -25,
                        MerchantName = "Electric bill",
                    },
                ]
            );
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service =>
                service.HasEnabledDestinationAsync(
                    user.Id,
                    NotificationEventTypes.RecurringReminder
                )
            )
            .ReturnsAsync(false);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var service = CreateService(
            context,
            recurringRuleService.Object,
            dispatcher.Object,
            nowProvider.Object
        );

        await service.CheckAfterSyncAsync(user.Id);

        var lastReminderDate = await context.ApplicationUsers
            .Where(applicationUser => applicationUser.Id == user.Id)
            .Select(applicationUser => applicationUser.LastRecurringReminderDate)
            .SingleAsync();
        lastReminderDate.Should().BeNull();
        dispatcher.Verify(
            item => item.DispatchAsync(It.IsAny<NotificationEvent>()),
            Times.Never
        );
    }

    private static RecurringReminderNotificationService CreateService(
        UserDataContext context,
        IRecurringRuleService recurringRuleService,
        INotificationDispatcher dispatcher,
        INowProvider nowProvider
    ) =>
        new(
            context,
            recurringRuleService,
            dispatcher,
            nowProvider,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            Mock.Of<ILogger<RecurringReminderNotificationService>>()
        );
}
