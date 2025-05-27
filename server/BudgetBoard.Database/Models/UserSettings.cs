namespace BudgetBoard.Database.Models;

public class UserSettings()
{
    public Guid ID { get; set; }
    public char Currency { get; set; } = '$';
    public Guid UserID { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
