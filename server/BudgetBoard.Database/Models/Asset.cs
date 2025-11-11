namespace BudgetBoard.Database.Models;

public class Asset
{
    public Guid ID { get; set; }
    public required string Name { get; set; }
    public DateTime? PurchaseDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SellDate { get; set; } = null;
    public decimal? SellPrice { get; set; } = null;
    public bool Hide { get; set; } = false;
    public DateTime? Deleted { get; set; } = null;
    public int Index { get; set; } = 0;
    public ICollection<Value> Values { get; set; } = [];
    public required Guid UserID { get; set; }
    public ApplicationUser? User { get; set; } = null!;
}
