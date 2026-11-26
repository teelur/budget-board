using System.Text.Json;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using BudgetBoard.WebAPI.Resources;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NotificationDestinationServiceTests
{
    [Fact]
    public async Task Destinations_AreUserScopedAndSummaryOmitsConfiguration()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var otherUserId = Guid.NewGuid();
        var request = new NotificationDestinationRequest
        {
            Name = "Example",
            Url = "https://example.test/secret",
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer secret" },
            EventTypes = [NotificationEventTypes.SyncFailed],
        };

        var created = await service.CreateAsync(userId, request, CancellationToken.None);
        var stored = await helper.UserDataContext.NotificationDestinations.SingleAsync();

        var storedConfiguration = JsonDocument.Parse(stored.Configuration);
        storedConfiguration.RootElement.GetProperty("Url").GetString()
            .Should().Be("https://example.test/secret");
        storedConfiguration.RootElement.GetProperty("Headers")
            .GetProperty("Authorization").GetString().Should().Be("Bearer secret");
        JsonSerializer.Serialize(created).Should().NotContain("secret");
        (await service.ReadAsync(otherUserId, CancellationToken.None)).Should().BeEmpty();
        (await service.ReadAsync(otherUserId, created.Id, CancellationToken.None)).Should().BeNull();
        (await service.UpdateAsync(otherUserId, created.Id, request, CancellationToken.None)).Should().BeNull();
        (await service.DeleteAsync(otherUserId, created.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task Update_WhenSecretsAreOmitted_RetainsExistingConfiguration()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var created = await service.CreateAsync(
            userId,
            new NotificationDestinationRequest
            {
                Name = "Example",
                Url = "https://example.test/secret",
                EventTypes = [NotificationEventTypes.SyncFailed],
            },
            CancellationToken.None
        );

        var updated = await service.UpdateAsync(
            userId,
            created.Id,
            new NotificationDestinationRequest { Name = "Updated", EventTypes = [] },
            CancellationToken.None
        );

        updated!.Name.Should().Be("Updated");
        updated.EventTypes.Should().BeEmpty();
        (await helper.UserDataContext.NotificationDestinations.SingleAsync())
            .Configuration.Should().Contain("https://example.test/secret");
    }

    [Fact]
    public async Task ReadDetails_ReturnsConfigurationOnlyToOwningUser()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var created = await service.CreateAsync(
            userId,
            new NotificationDestinationRequest
            {
                Name = "Example",
                Url = "https://example.test/secret",
                Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer secret",
                },
                ContentType = "text/plain",
                BodyTemplate = "{{message}}",
                EventTypes = [NotificationEventTypes.SyncFailed],
            },
            CancellationToken.None
        );

        var details = await service.ReadAsync(userId, created.Id, CancellationToken.None);
        var summaries = await service.ReadAsync(userId, CancellationToken.None);

        details.Should().NotBeNull();
        details!.Url.Should().Be("https://example.test/secret");
        details.Headers.Should().ContainKey("Authorization").WhoseValue.Should().Be("Bearer secret");
        details.ContentType.Should().Be("text/plain");
        details.BodyTemplate.Should().Be("{{message}}");
        JsonSerializer.Serialize(summaries).Should().NotContain("secret");
        (await service.ReadAsync(Guid.NewGuid(), created.Id, CancellationToken.None)).Should().BeNull();
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("http://[::ffff:169.254.169.254]/")]
    [InlineData("http://[fd00:ec2::254]/")]
    [InlineData("https://user:password@example.test/")]
    public async Task Create_RejectsInvalidDestinations(string url)
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var request = new NotificationDestinationRequest { Name = "Example", Url = url, EventTypes = [] };

        var action = () => service.CreateAsync(helper.demoUser.Id, request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_RejectsInvalidJsonTemplate()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var request = new NotificationDestinationRequest
        {
            Name = "Example",
            Url = "https://example.test/",
            BodyTemplate = "{\"message\":{{message}}}",
            EventTypes = [],
        };

        var action = () => service.CreateAsync(helper.demoUser.Id, request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    private static NotificationDestinationService CreateService(TestHelper helper)
    {
        var sender = new NotificationWebhookSender(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
        return new NotificationDestinationService(
            helper.UserDataContext, sender, TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
    }
}
