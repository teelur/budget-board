using System.Text.Json.Serialization;

namespace BudgetBoard.WebAPI.Models
{
    public class OidcCallbackRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("redirect_uri")]
        public string RedirectUri { get; set; } = string.Empty;
    }
}
