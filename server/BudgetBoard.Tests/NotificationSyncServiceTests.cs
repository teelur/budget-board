using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NotificationSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_WhenErrorsAreReturned_SendsOneFailureNotification()
    {
        var userId = Guid.NewGuid();
        var sync = new Mock<ISyncService>();
        var errors = new[]
        {
            new SyncError("SimpleFIN", "Private error"),
            new SyncError("LunchFlow", "Second private error"),
        };
        sync.Setup(s => s.SyncAsync(userId)).ReturnsAsync(errors);
        var dispatcher = new Mock<INotificationDispatcher>();
        var reminderService = new Mock<INotificationReminderService>();
        var service = CreateService(sync.Object, dispatcher.Object, reminderService.Object);

        var result = await service.SyncAsync(userId);

        result.Should().BeSameAs(errors);
        dispatcher.Verify(d => d.DispatchAsync(It.Is<NotificationEvent>(e =>
            e.UserId == userId && e.Type == NotificationEventTypes.SyncFailed
            && e.Message == "NotificationSyncFailureWithErrorCount [2]"
            && e.FinancialDetails == "SimpleFIN: Private error\nLunchFlow: Second private error"
        )), Times.Once);
        reminderService.Verify(
            reminder => reminder.CheckAfterSyncAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenReturnedErrorsAreLong_TruncatesDetails()
    {
        var sync = new Mock<ISyncService>();
        sync.Setup(service => service.SyncAsync(It.IsAny<Guid>()))
            .ReturnsAsync([new SyncError("provider", new string('x', 3000))]);
        var dispatcher = new Mock<INotificationDispatcher>();
        var service = CreateService(sync.Object, dispatcher.Object);
        NotificationEvent? notification = null;
        dispatcher.Setup(dispatch => dispatch.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Callback<NotificationEvent>(eventData => notification = eventData)
            .Returns(Task.CompletedTask);

        await service.SyncAsync(Guid.NewGuid());

        notification!.FinancialDetails.Should().HaveLength(2000);
        notification.FinancialDetails.Should().EndWith("…");
    }

    [Fact]
    public async Task SyncAsync_WhenSyncThrows_NotifiesAndPreservesOriginalError()
    {
        var userId = Guid.NewGuid();
        var sync = new Mock<ISyncService>();
        var error = new InvalidOperationException("sync failed");
        sync.Setup(s => s.SyncAsync(userId)).ThrowsAsync(error);
        var dispatcher = new Mock<INotificationDispatcher>();
        var reminderService = new Mock<INotificationReminderService>();
        NotificationEvent? notification = null;
        dispatcher.Setup(d => d.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Callback<NotificationEvent>(eventData => notification = eventData)
            .ThrowsAsync(new InvalidOperationException("delivery failed"));
        var service = CreateService(sync.Object, dispatcher.Object, reminderService.Object);

        var action = () => service.SyncAsync(userId);

        (await action.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should().BeSameAs(error);
        notification!.FinancialDetails.Should().Be(
            "NotificationUnexpectedSyncError [InvalidOperationException]"
        );
        notification.FinancialDetails.Should().NotContain("sync failed");
        dispatcher.Verify(d => d.DispatchAsync(It.IsAny<NotificationEvent>()), Times.Once);
        reminderService.Verify(
            reminder => reminder.CheckAfterSyncAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenServiceExceptionIsThrown_IncludesSafeErrorMessage()
    {
        var sync = new Mock<ISyncService>();
        sync.Setup(service => service.SyncAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new BudgetBoardServiceException("Invalid provider configuration"));
        var dispatcher = new Mock<INotificationDispatcher>();
        NotificationEvent? notification = null;
        dispatcher.Setup(dispatch => dispatch.DispatchAsync(It.IsAny<NotificationEvent>()))
            .Callback<NotificationEvent>(eventData => notification = eventData)
            .Returns(Task.CompletedTask);
        var service = CreateService(sync.Object, dispatcher.Object);

        await FluentActions.Awaiting(() => service.SyncAsync(Guid.NewGuid()))
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();

        notification!.FinancialDetails.Should().Be("Invalid provider configuration");
    }

    [Fact]
    public async Task SyncAsync_WhenSuccessful_DoesNotNotify()
    {
        var sync = new Mock<ISyncService>();
        sync.Setup(s => s.SyncAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        var dispatcher = new Mock<INotificationDispatcher>();
        var userId = Guid.NewGuid();
        var reminderService = new Mock<INotificationReminderService>();
        var service = CreateService(sync.Object, dispatcher.Object, reminderService.Object);

        await service.SyncAsync(userId);

        dispatcher.Verify(d => d.DispatchAsync(It.IsAny<NotificationEvent>()), Times.Never);
        reminderService.Verify(
            reminder => reminder.CheckAfterSyncAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_CompletesNotificationScopeAfterSyncFinishes()
    {
        var userId = Guid.NewGuid();
        var order = new List<string>();
        var sync = new Mock<ISyncService>();
        sync.Setup(service => service.SyncAsync(userId))
            .Returns(() =>
            {
                order.Add("sync finished");
                return Task.FromResult<IReadOnlyList<SyncError>>([]);
            });
        var activityScope = new Mock<INotificationActivityScope>();
        activityScope
            .Setup(scope => scope.CompleteAsync())
            .Callback(() => order.Add("activity completed"))
            .Returns(Task.CompletedTask);
        activityScope.Setup(scope => scope.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var activityService = new Mock<INotificationActivityService>();
        activityService
            .Setup(service => service.BeginAsync(userId))
            .ReturnsAsync(activityScope.Object);
        var service = CreateService(
            sync.Object,
            Mock.Of<INotificationDispatcher>(),
            activityService: activityService.Object
        );

        await service.SyncAsync(userId);

        order.Should().ContainInOrder("sync finished", "activity completed");
    }

    private static NotificationSyncService CreateService(
        ISyncService sync,
        INotificationDispatcher dispatcher,
        INotificationReminderService? reminderService = null,
        INotificationActivityService? activityService = null
    ) => new(
        sync,
        dispatcher,
        TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
        TestHelper.CreateMockLocalizer<ApiLogStrings>(),
        Mock.Of<ILogger<NotificationSyncService>>(),
        activityService ?? TestHelper.CreateMockNotificationActivityService(),
        reminderService ?? Mock.Of<INotificationReminderService>()
    );
}
