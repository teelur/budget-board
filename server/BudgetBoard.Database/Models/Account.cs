namespace BudgetBoard.Database.Models;

public class Account
{
    public Guid ID { get; set; }
    public string? SyncID { get; set; }
    public required string Name { get; set; }
    public Guid? InstitutionID { get; set; }
    public Institution Institution { get; set; } = null!;
    public string Type { get; set; } = "";
    public string Subtype { get; set; } = "";
    public bool HideTransactions { get; set; } = false;
    public bool HideAccount { get; set; } = false;
    public DateTime? Deleted { get; set; } = null;
    public int Index { get; set; } = 0;
    public decimal? InterestRate { get; set; } = null;
    public string Source { get; set; } = string.Empty;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<Balance> Balances { get; set; } = [];
    public required Guid UserID { get; set; }
    public ApplicationUser? User { get; set; } = null!;
}
