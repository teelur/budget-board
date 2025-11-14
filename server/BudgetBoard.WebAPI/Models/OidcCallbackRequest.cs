namespace BudgetBoard.WebAPI.Models
{
    public class OidcCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? RedirectUri { get; set; }
        public string? State { get; set; }
    }
}
