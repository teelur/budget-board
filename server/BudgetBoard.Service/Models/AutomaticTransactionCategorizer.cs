namespace BudgetBoard.Service.Models;

public interface ITrainAutoCategorizerRequest
{
    DateOnly? StartDate { get; }
    DateOnly? EndDate { get; }
}

public class TrainAutoCategorizerRequest() : ITrainAutoCategorizerRequest
{
    public DateOnly? StartDate { get; set; } = null;
    public DateOnly? EndDate { get; set; } = null;
}

public interface ITrainAutoCategorizerResponse
{
    bool Success { get; }
    string Error { get; }
}

public class TrainAutoCategorizerResponse : ITrainAutoCategorizerResponse
{
    public bool Success { get; set; } = true;
    public string Error { get; set; } = string.Empty;
}