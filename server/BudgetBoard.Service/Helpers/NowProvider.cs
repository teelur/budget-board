namespace BudgetBoard.Service.Helpers;

public interface INowProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

public class NowProvider : INowProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
