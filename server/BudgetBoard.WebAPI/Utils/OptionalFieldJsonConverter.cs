using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.Service.Models;

namespace BudgetBoard.WebAPI.Utils;

public class OptionalFieldJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(OptionalField<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalFieldJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class OptionalFieldJsonConverter<T> : JsonConverter<OptionalField<T>>
{
    public override OptionalField<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new JsonException(
                    $"Null is not valid for non-nullable type {typeof(T).Name}."
                );
            }

            return new OptionalField<T>(default);
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new OptionalField<T>(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        OptionalField<T> value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
