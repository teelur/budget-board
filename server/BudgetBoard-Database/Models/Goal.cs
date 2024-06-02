﻿namespace BudgetBoard.Database.Models;

public class Goal
{
    public Guid ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? CompleteDate { get; set; }
    public float Amount { get; set; } = 0.0f;
    public float? InitialAmount { get; set; }
    public float? MonthlyContribution { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public required Guid UserID { get; set; }
    public ApplicationUser? User { get; set; } = null!;
}

public class NewGoal
{
    public required string Name { get; set; }
    public DateTime? CompleteDate { get; set; }
    public required float Amount { get; set; }
    public float? InitialAmount { get; set; }
    public float? MonthlyContribution { get; set; }
    public required ICollection<string> AccountIds { get; set; }
}
