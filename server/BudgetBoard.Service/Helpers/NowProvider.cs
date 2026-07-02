namespace BudgetBoard.Service.Helpers;

public interface INowProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}

public class NowProvider : INowProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
