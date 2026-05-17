namespace BudgetBoard.Service.Helpers;

public interface INowProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

public class NowProvider(TimeZoneInfo timeZone) : INowProvider
{
    private static readonly Lazy<NowProvider> _instance = new(() =>
        new NowProvider(TimeZoneInfo.Local)
    );
    public static NowProvider Instance => _instance.Value;

    private readonly TimeZoneInfo _timeZone = timeZone;

    public DateTime Now => TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZone);
    public DateTime UtcNow => DateTime.UtcNow;
}
