namespace BudgetBoard.Service.Models;

public class BudgetBoardServiceException : Exception
{
    public int StatusCode { get; }

    public BudgetBoardServiceException()
    {
        StatusCode = 500;
    }

    public BudgetBoardServiceException(string? message) : base(message)
    {
        StatusCode = 500;
    }

    public BudgetBoardServiceException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = 500;
    }

    public BudgetBoardServiceException(string? message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public BudgetBoardServiceException(string? message, int statusCode, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
