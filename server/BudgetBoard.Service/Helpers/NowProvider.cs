namespace BudgetBoard.Service.Helpers;

public interface INowProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

public class NowProvider : INowProvider
{
    private static readonly Lazy<NowProvider> _instance = new(() => new NowProvider());
    public static NowProvider Instance => _instance.Value;
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
