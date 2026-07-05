namespace BudgetBoard.Service.Models;

public class BudgetBoardServiceException : Exception
{
    public BudgetBoardServiceException() { }

    public BudgetBoardServiceException(string? message)
        : base(message) { }

    public BudgetBoardServiceException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
