namespace BudgetBoard.Database.Models;

public class Property
{
    public Guid ID { get; set; }
    public required string Name { get; set; }
    public DateTime? PurchasedDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SoldDate { get; set; } = null;
    public decimal? SoldPrice { get; set; } = null;
    public bool HideProperty { get; set; } = false;
    public DateTime? Deleted { get; set; } = null;
    public int Index { get; set; } = 0;
    public ICollection<Value> Values { get; set; } = [];
    public required Guid UserID { get; set; }
    public ApplicationUser? User { get; set; } = null!;
}