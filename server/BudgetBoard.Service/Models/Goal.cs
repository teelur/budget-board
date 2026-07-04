using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IGoalCreateRequest
{
    string Name { get; }
    DateOnly? CompleteDate { get; }
    decimal Amount { get; }
    bool ApplyExistingBalanceTowardsGoal { get; }
    decimal? MonthlyContribution { get; }
    IEnumerable<Guid> AccountIds { get; }
}

public class GoalCreateRequest : IGoalCreateRequest
{
    public required string Name { get; set; }
    public DateOnly? CompleteDate { get; set; } = null;
    public required decimal Amount { get; set; } = 0.0M;
    public bool ApplyExistingBalanceTowardsGoal { get; set; } = false;
    public decimal? MonthlyContribution { get; set; } = null;
    public required IEnumerable<Guid> AccountIds { get; set; } = [];
}

public interface IGoalUpdateRequest
{
    Guid ID { get; }
    string? Name { get; }
    OptionalField<DateOnly?> CompleteDate { get; }
    decimal? Amount { get; }
    OptionalField<decimal?> MonthlyContribution { get; }
}

public class GoalUpdateRequest : IGoalUpdateRequest
{
    public Guid ID { get; set; }
    public string? Name { get; set; } = string.Empty;
    public OptionalField<DateOnly?> CompleteDate { get; set; } = new OptionalField<DateOnly?>();
    public decimal? Amount { get; set; } = null;
    public OptionalField<decimal?> MonthlyContribution { get; set; } =
        new OptionalField<decimal?>();
}

public interface IGoalResponse
{
    Guid ID { get; }
    string Name { get; }
    DateOnly CompleteDate { get; }
    bool IsCompleteDateEditable { get; }
    decimal Amount { get; }
    decimal InitialAmount { get; }
    decimal MonthlyContribution { get; }
    bool IsMonthlyContributionEditable { get; }
    decimal MonthlyContributionProgress { get; }
    decimal? InterestRate { get; }
    DateOnly? Completed { get; }
    decimal PercentComplete { get; }
    IEnumerable<IAccountResponse> Accounts { get; }
    Guid UserID { get; }
}

public class GoalResponse : IGoalResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateOnly CompleteDate { get; set; } = DateOnly.FromDateTime(DateTime.UnixEpoch);
    public bool IsCompleteDateEditable { get; set; } = false;
    public decimal Amount { get; set; } = 0.0M;
    public decimal InitialAmount { get; set; } = 0.0M;
    public decimal MonthlyContribution { get; set; } = 0.0M;
    public bool IsMonthlyContributionEditable { get; set; } = false;
    public decimal MonthlyContributionProgress { get; set; } = 0.0M;
    public decimal? InterestRate { get; set; } = null;
    public DateOnly? Completed { get; set; } = null;
    public decimal PercentComplete { get; set; } = 0.0M;
    public IEnumerable<IAccountResponse> Accounts { get; set; } = [];
    public Guid UserID { get; set; } = Guid.NewGuid();

    public GoalResponse(Goal goal)
    {
        ID = goal.ID;
        Name = goal.Name;
        CompleteDate = goal.CompleteDate ?? DateOnly.FromDateTime(DateTime.UnixEpoch);
        IsCompleteDateEditable = goal.CompleteDate != null;
        Amount = goal.Amount;
        InitialAmount = goal.InitialAmount;
        MonthlyContribution = goal.MonthlyContribution ?? 0;
        IsMonthlyContributionEditable = goal.MonthlyContribution != null;
        MonthlyContributionProgress = 0;
        InterestRate = null;
        Completed = goal.Completed;
        PercentComplete = 0.0M;
        Accounts = goal.Accounts.Select(a => new AccountResponse(a));
        UserID = goal.UserID;
    }
}
