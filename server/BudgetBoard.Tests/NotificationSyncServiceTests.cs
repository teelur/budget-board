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
        var errors = new[] { new SyncError("provider", "private error") };
        sync.Setup(s => s.SyncAsync(userId)).ReturnsAsync(errors);
        var dispatcher = new Mock<INotificationDispatcher>();
        var service = CreateService(sync.Object, dispatcher.Object);

        var result = await service.SyncAsync(userId);

        result.Should().BeSameAs(errors);
        dispatcher.Verify(d => d.DispatchAsync(It.Is<NotificationEvent>(e =>
            e.UserId == userId && e.Type == NotificationEventTypes.SyncFailed
            && !e.Message.Contains("private error")
        )), Times.Once);
    }

    [Fact]
    public async Task SyncAsync_WhenSyncThrows_NotifiesAndPreservesOriginalError()
    {
        var userId = Guid.NewGuid();
        var sync = new Mock<ISyncService>();
        var error = new InvalidOperationException("sync failed");
        sync.Setup(s => s.SyncAsync(userId)).ThrowsAsync(error);
        var dispatcher = new Mock<INotificationDispatcher>();
        dispatcher.Setup(d => d.DispatchAsync(It.IsAny<NotificationEvent>()))
            .ThrowsAsync(new InvalidOperationException("delivery failed"));
        var service = CreateService(sync.Object, dispatcher.Object);

        var action = () => service.SyncAsync(userId);

        (await action.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should().BeSameAs(error);
        dispatcher.Verify(d => d.DispatchAsync(It.IsAny<NotificationEvent>()), Times.Once);
    }

    [Fact]
    public async Task SyncAsync_WhenSuccessful_DoesNotNotify()
    {
        var sync = new Mock<ISyncService>();
        sync.Setup(s => s.SyncAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        var dispatcher = new Mock<INotificationDispatcher>();
        var service = CreateService(sync.Object, dispatcher.Object);

        await service.SyncAsync(Guid.NewGuid());

        dispatcher.Verify(d => d.DispatchAsync(It.IsAny<NotificationEvent>()), Times.Never);
    }

    private static NotificationSyncService CreateService(
        ISyncService sync,
        INotificationDispatcher dispatcher
    ) => new(
        sync,
        dispatcher,
        TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
        TestHelper.CreateMockLocalizer<ApiLogStrings>(),
        Mock.Of<ILogger<NotificationSyncService>>(),
        TestHelper.CreateMockNotificationActivityService()
    );
}
