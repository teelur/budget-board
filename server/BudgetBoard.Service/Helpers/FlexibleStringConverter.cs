using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Helpers;

public class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        // If the token is a string, return as is
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();

        // If the token is a number, convert to string
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble().ToString();

        // If the token is null, return null
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Otherwise, throw
        throw new JsonException($"Unexpected token {reader.TokenType} when parsing a string.");
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
