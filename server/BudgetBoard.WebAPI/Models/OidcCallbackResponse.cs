namespace BudgetBoard.WebAPI.Models
{
    public class OidcCallbackResponse
    {
        public bool Success { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }
    }
}
