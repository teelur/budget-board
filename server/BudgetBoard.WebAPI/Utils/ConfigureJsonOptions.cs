using System.Text.Json.Serialization;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace BudgetBoard.WebAPI.Utils;

public class ConfigureJsonOptions(IStringLocalizer<ApiResponseStrings> localizer)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new FlexibleStringConverter(localizer));
    }
}
