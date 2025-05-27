namespace BudgetBoard.Database.Models;

public class UserSettings()
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public char Currency { get; set; } = '$';
    public Guid UserID { get; set; } = Guid.Empty;
    public ApplicationUser? User { get; set; }
}
