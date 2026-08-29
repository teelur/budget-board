using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public static class RecurringCadenceUnitValues
{
    public const string Day = "Day";
    public const string Week = "Week";
    public const string Month = "Month";
    public const string Year = "Year";
}

public static class RecurringCadenceModeValues
{
    public const string Interval = "Interval";
    public const string PerUnit = "PerUnit";
}

public sealed class RecurringCadence
{
    public int Version { get; set; }
    public string? Unit { get; set; }
    public int Interval { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mode { get; set; }
}

public sealed class RecurringCadenceValidationException : Exception
{
    public RecurringCadenceValidationException(string message)
        : base(message) { }

    public RecurringCadenceValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public static class RecurringCadenceSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public static string Serialize(RecurringCadence cadence)
    {
        Validate(cadence);
        return JsonSerializer.Serialize(cadence, SerializerOptions);
    }

    public static RecurringCadence Deserialize(string cadence)
    {
        if (string.IsNullOrWhiteSpace(cadence))
        {
            throw new RecurringCadenceValidationException("Cadence is required.");
        }

        try
        {
            var definition = JsonSerializer.Deserialize<RecurringCadence>(cadence, SerializerOptions);
            Validate(definition);
            return definition!;
        }
        catch (JsonException exception)
        {
            throw new RecurringCadenceValidationException(
                "Cadence must be a valid recurrence definition.",
                exception
            );
        }
    }

    public static void Validate(RecurringCadence? cadence)
    {
        if (cadence is null)
        {
            throw new RecurringCadenceValidationException("Cadence is required.");
        }

        if (cadence.Version != 1)
        {
            throw new RecurringCadenceValidationException("Cadence version is not supported.");
        }

        if (
            cadence.Unit is not RecurringCadenceUnitValues.Day
                and not RecurringCadenceUnitValues.Week
                and not RecurringCadenceUnitValues.Month
                and not RecurringCadenceUnitValues.Year
        )
        {
            throw new RecurringCadenceValidationException("Cadence unit is not supported.");
        }

        if (cadence.Interval <= 0)
        {
            throw new RecurringCadenceValidationException("Cadence interval must be positive.");
        }

        var mode = cadence.Mode ?? RecurringCadenceModeValues.Interval;
        if (mode is not RecurringCadenceModeValues.Interval and not RecurringCadenceModeValues.PerUnit)
        {
            throw new RecurringCadenceValidationException("Cadence mode is not supported.");
        }

        if (mode == RecurringCadenceModeValues.PerUnit)
        {
            var maximumOccurrences = cadence.Unit switch
            {
                RecurringCadenceUnitValues.Day => 1,
                RecurringCadenceUnitValues.Week => 7,
                RecurringCadenceUnitValues.Month => 31,
                RecurringCadenceUnitValues.Year => 366,
                _ => 0,
            };

            if (cadence.Interval > maximumOccurrences)
            {
                throw new RecurringCadenceValidationException(
                    "Cadence occurrences per unit exceed the supported maximum."
                );
            }
        }
    }
}