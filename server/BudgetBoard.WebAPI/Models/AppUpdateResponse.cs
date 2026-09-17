namespace BudgetBoard.WebAPI.Models;

public static class AppUpdateChannels
{
    public const string Stable = "stable";
    public const string Dev = "dev";

    public static bool TryNormalize(string? channel, out string normalizedChannel)
    {
        normalizedChannel = channel?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalizedChannel is Stable or Dev;
    }
}

/// <summary>Latest update metadata for one application build channel.</summary>
public sealed record AppUpdateResponse(string Channel, string LatestVersion, string Url);
