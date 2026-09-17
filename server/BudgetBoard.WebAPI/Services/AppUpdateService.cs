using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BudgetBoard.WebAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetBoard.WebAPI.Services;

public sealed class AppUpdateService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger<AppUpdateService> logger
) : IAppUpdateService
{
    public const string GitHubHttpClientName = "GitHub";

    private const string Repository = "teelur/budget-board";
    private const string DevWorkflowFile = "docker-image-dev-build.yml";
    private const string CacheKeyPrefix = "app-update";
    private static readonly TimeSpan SuccessCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly Regex StableVersionPattern = new(
        @"^v?\d+\.\d+\.\d+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );
    private static readonly Regex DevCommitPattern = new(
        @"^[0-9a-f]{7,40}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );

    private readonly SemaphoreSlim _stableRefreshLock = new(1, 1);
    private readonly SemaphoreSlim _devRefreshLock = new(1, 1);

    public async Task<AppUpdateResponse?> GetLatestAsync(
        string channel,
        CancellationToken cancellationToken
    )
    {
        if (!AppUpdateChannels.TryNormalize(channel, out var normalizedChannel))
        {
            throw new ArgumentException("The update channel is not supported.", nameof(channel));
        }

        var cacheKey = $"{CacheKeyPrefix}:{normalizedChannel}";
        if (cache.TryGetValue(cacheKey, out CachedUpdate? cachedUpdate))
        {
            return cachedUpdate?.Response;
        }

        var refreshLock =
            normalizedChannel == AppUpdateChannels.Stable ? _stableRefreshLock : _devRefreshLock;

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(cacheKey, out cachedUpdate))
            {
                return cachedUpdate?.Response;
            }

            var response = await FetchLatestAsync(normalizedChannel, cancellationToken);
            cache.Set(
                cacheKey,
                new CachedUpdate(response),
                response is null ? FailureCacheDuration : SuccessCacheDuration
            );
            return response;
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private async Task<AppUpdateResponse?> FetchLatestAsync(
        string channel,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return channel == AppUpdateChannels.Stable
                ? await FetchStableUpdateAsync(cancellationToken)
                : await FetchDevUpdateAsync(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "GitHub update metadata request failed for {Channel}.",
                channel
            );
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "GitHub update metadata was invalid for {Channel}.",
                channel
            );
            return null;
        }
    }

    private async Task<AppUpdateResponse?> FetchStableUpdateAsync(
        CancellationToken cancellationToken
    )
    {
        var release = await GetGitHubResponseAsync<GitHubReleaseResponse>(
            $"repos/{Repository}/releases/latest",
            cancellationToken
        );

        if (
            release?.TagName is not { } tagName
            || release.HtmlUrl is not { } htmlUrl
            || release.Draft == true
            || release.Prerelease == true
            || !StableVersionPattern.IsMatch(tagName)
            || !IsValidUrl(htmlUrl)
        )
        {
            return null;
        }

        return new AppUpdateResponse(AppUpdateChannels.Stable, tagName.Trim(), htmlUrl);
    }

    private async Task<AppUpdateResponse?> FetchDevUpdateAsync(CancellationToken cancellationToken)
    {
        var workflowRuns = await GetGitHubResponseAsync<GitHubWorkflowRunsResponse>(
            $"repos/{Repository}/actions/workflows/{DevWorkflowFile}/runs?branch=main&status=success&per_page=1",
            cancellationToken
        );
        var latestRun = workflowRuns?.WorkflowRuns?.FirstOrDefault();

        if (
            latestRun?.HeadSha is not { } headSha
            || latestRun.HtmlUrl is not { } htmlUrl
            || latestRun.Conclusion != "success"
            || !DevCommitPattern.IsMatch(headSha)
            || !IsValidUrl(htmlUrl)
        )
        {
            return null;
        }

        return new AppUpdateResponse(AppUpdateChannels.Dev, $"sha-{headSha[..7]}", htmlUrl);
    }

    private async Task<T?> GetGitHubResponseAsync<T>(
        string requestUri,
        CancellationToken cancellationToken
    )
    {
        var client = httpClientFactory.CreateClient(GitHubHttpClientName);
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private static bool IsValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    private sealed record CachedUpdate(AppUpdateResponse? Response);

    private sealed record GitHubReleaseResponse(
        [property: JsonPropertyName("tag_name")] string? TagName,
        [property: JsonPropertyName("html_url")] string? HtmlUrl,
        [property: JsonPropertyName("draft")] bool? Draft,
        [property: JsonPropertyName("prerelease")] bool? Prerelease
    );

    private sealed record GitHubWorkflowRunsResponse(
        [property: JsonPropertyName("workflow_runs")] List<GitHubWorkflowRun>? WorkflowRuns
    );

    private sealed record GitHubWorkflowRun(
        [property: JsonPropertyName("head_sha")] string? HeadSha,
        [property: JsonPropertyName("html_url")] string? HtmlUrl,
        [property: JsonPropertyName("conclusion")] string? Conclusion
    );
}
