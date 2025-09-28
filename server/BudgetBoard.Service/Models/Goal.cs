using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IGoalCreateRequest
{
    string Name { get; set; }
    DateTime? CompleteDate { get; set; }
    decimal Amount { get; set; }
    decimal? InitialAmount { get; set; }
    decimal? MonthlyContribution { get; set; }
    IEnumerable<Guid> AccountIds { get; set; }
}

public class GoalCreateRequest : IGoalCreateRequest
{
    public required string Name { get; set; }
    public DateTime? CompleteDate { get; set; }
    public required decimal Amount { get; set; }
    public decimal? InitialAmount { get; set; }
    public decimal? MonthlyContribution { get; set; }
    public required IEnumerable<Guid> AccountIds { get; set; }

    [JsonConstructor]
    public GoalCreateRequest()
    {
        Name = string.Empty;
        CompleteDate = null;
        Amount = 0.0M;
        InitialAmount = null;
        MonthlyContribution = null;
        AccountIds = [];
    }
}

public interface IGoalUpdateRequest
{
    Guid ID { get; set; }
    string Name { get; set; }
    DateTime? CompleteDate { get; set; }
    bool IsCompleteDateEditable { get; set; }
    decimal Amount { get; set; }
    decimal? MonthlyContribution { get; set; }
    bool IsMonthlyContributionEditable { get; set; }
}

public class GoalUpdateRequest : IGoalUpdateRequest
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public DateTime? CompleteDate { get; set; }
    public bool IsCompleteDateEditable { get; set; }
    public decimal Amount { get; set; }
    public decimal? MonthlyContribution { get; set; }
    public bool IsMonthlyContributionEditable { get; set; }

    [JsonConstructor]
    public GoalUpdateRequest()
    {
        Name = string.Empty;
        CompleteDate = null;
        Amount = 0.0M;
        MonthlyContribution = null;
    }
}

public interface IGoalResponse
{
    Guid ID { get; set; }
    string Name { get; set; }
    DateTime CompleteDate { get; set; }
    bool IsCompleteDateEditable { get; set; }
    decimal Amount { get; set; }
    decimal InitialAmount { get; set; }
    decimal MonthlyContribution { get; set; }
    bool IsMonthlyContributionEditable { get; set; }
    decimal MonthlyContributionProgress { get; set; }
    decimal? InterestRate { get; set; }
    DateTime? Completed { get; set; }
    decimal PercentComplete { get; set; }
    IEnumerable<IAccountResponse> Accounts { get; set; }
    Guid UserID { get; set; }
}

public class GoalResponse : IGoalResponse
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public DateTime CompleteDate { get; set; }
    public bool IsCompleteDateEditable { get; set; }
    public decimal Amount { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal MonthlyContribution { get; set; }
    public bool IsMonthlyContributionEditable { get; set; }
    public decimal MonthlyContributionProgress { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime? Completed { get; set; }
    public decimal PercentComplete { get; set; }
    public IEnumerable<IAccountResponse> Accounts { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public GoalResponse()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
        CompleteDate = DateTime.UnixEpoch;
        IsCompleteDateEditable = false;
        Amount = 0.0M;
        InitialAmount = 0;
        MonthlyContribution = 0;
        IsMonthlyContributionEditable = false;
        MonthlyContributionProgress = 0;
        InterestRate = null;
        Completed = null;
        PercentComplete = 0.0M;
        Accounts = [];
        UserID = Guid.NewGuid();
    }

    public GoalResponse(Goal goal)
    {
        ID = goal.ID;
        Name = goal.Name;
        CompleteDate = goal.CompleteDate ?? DateTime.UnixEpoch;
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
