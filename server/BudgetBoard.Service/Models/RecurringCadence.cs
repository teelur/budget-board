using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

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
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    )
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public static string Serialize(
        RecurringCadence cadence,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        return JsonSerializer.Serialize(Normalize(cadence, responseLocalizer), SerializerOptions);
    }

    public static RecurringCadence Deserialize(
        string cadence,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (string.IsNullOrWhiteSpace(cadence))
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceRequiredError"]
            );
        }

        try
        {
            var definition = JsonSerializer.Deserialize<RecurringCadence>(
                cadence,
                SerializerOptions
            );
            return Normalize(definition, responseLocalizer);
        }
        catch (JsonException exception)
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceInvalidDefinitionError"],
                exception
            );
        }
    }

    public static void Validate(
        RecurringCadence? cadence,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (cadence is null)
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceRequiredError"]
            );
        }

        if (cadence.Version != 1)
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceUnsupportedVersionError"]
            );
        }

        if (GetCanonicalUnit(cadence.Unit) is null)
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceUnsupportedUnitError"]
            );
        }

        if (cadence.Interval <= 0)
        {
            throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceInvalidIntervalError"]
            );
        }

        var mode =
            (
                cadence.Mode is null
                    ? RecurringCadenceModeValues.Interval
                    : GetCanonicalMode(cadence.Mode)
            )
            ?? throw new RecurringCadenceValidationException(
                responseLocalizer["RecurringCadenceUnsupportedModeError"]
            );
        if (mode == RecurringCadenceModeValues.PerUnit)
        {
            var maximumOccurrences = GetCanonicalUnit(cadence.Unit) switch
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
                    responseLocalizer["RecurringCadenceMaximumOccurrencesError"]
                );
            }
        }
    }

    public static RecurringCadence CreateDefault() =>
        new()
        {
            Version = 1,
            Unit = RecurringCadenceUnitValues.Month,
            Interval = 1,
        };

    private static RecurringCadence Normalize(
        RecurringCadence? cadence,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        Validate(cadence, responseLocalizer);

        var mode = cadence!.Mode is null ? null : GetCanonicalMode(cadence.Mode);
        return new RecurringCadence
        {
            Version = cadence.Version,
            Unit = GetCanonicalUnit(cadence.Unit),
            Interval = cadence.Interval,
            Mode = mode == RecurringCadenceModeValues.PerUnit ? mode : null,
        };
    }

    private static string? GetCanonicalUnit(string? unit) =>
        unit is null
            ? null
            : new[]
            {
                RecurringCadenceUnitValues.Day,
                RecurringCadenceUnitValues.Week,
                RecurringCadenceUnitValues.Month,
                RecurringCadenceUnitValues.Year,
            }.FirstOrDefault(value =>
                string.Equals(value, unit, StringComparison.OrdinalIgnoreCase)
            );

    private static string? GetCanonicalMode(string mode) =>
        new[]
        {
            RecurringCadenceModeValues.Interval,
            RecurringCadenceModeValues.PerUnit,
        }.FirstOrDefault(value => string.Equals(value, mode, StringComparison.OrdinalIgnoreCase));
}
