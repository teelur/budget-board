using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Jobs;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class RecurringReminderBackgroundJobTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOccurrenceMatchesConfiguredLeadTime_DispatchesReminder()
    {
        var helper = new TestHelper();
        var userId = helper.demoUser.Id;
        var today = new DateOnly(2026, 11, 29);
        helper.UserDataContext.UserSettings.Add(
            new UserSettings { UserID = userId, RecurringReminderLeadTimeDays = 5 }
        );
        await helper.UserDataContext.SaveChangesAsync();

        var occurrence = new RecurringForecastOccurrenceResponse
        {
            RuleID = Guid.NewGuid(),
            Date = today.AddDays(5),
            Amount = -25,
            MerchantName = "Electric bill",
        };
        var recurringRuleService = new Mock<IRecurringRuleService>();
        recurringRuleService
            .Setup(service => service.ReadForecastAsync(userId, new DateOnly(2026, 12, 1)))
            .ReturnsAsync((IReadOnlyList<RecurringForecastOccurrenceResponse>)[occurrence]);
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher
            .Setup(service => service.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Returns(Task.CompletedTask);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.Today).Returns(today);
        var job = new RecurringReminderBackgroundJob(
            helper.UserDataContext,
            recurringRuleService.Object,
            dispatcher.Object,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            Mock.Of<ILogger<RecurringReminderBackgroundJob>>()
        );

        await job.Execute(Mock.Of<IJobExecutionContext>(), CancellationToken.None);

        dispatcher.Verify(
            service =>
                service.DispatchAsync(
                    It.Is<NotificationEvent>(notification =>
                        notification.UserId == userId
                        && notification.Type == NotificationEventTypes.RecurringReminder
                        && notification.FinancialDetails!.Contains("Electric bill")
                    )
                ),
            Times.Once
        );
    }
}
