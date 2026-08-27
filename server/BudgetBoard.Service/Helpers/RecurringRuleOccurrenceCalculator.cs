using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Helpers;

public static class RecurringRuleOccurrenceCalculator
{
    public static IReadOnlyList<DateOnly> GetOccurrences(
        RecurringRule rule,
        DateOnly rangeStart,
        DateOnly rangeEnd
    )
    {
        if (rangeEnd < rangeStart || !rule.IsActive || rule.EndDate < rangeStart)
        {
            return [];
        }

        var start = Max(rangeStart, rule.StartDate);
        var end = Min(rangeEnd, rule.EndDate ?? rangeEnd);
        if (end < start)
        {
            return [];
        }

        return rule.Cadence switch
        {
            RecurringCadence.Weekly => GetWeeklyOccurrences(rule.StartDate, start, end, 7),
            RecurringCadence.Biweekly => GetWeeklyOccurrences(rule.StartDate, start, end, 14),
            RecurringCadence.Monthly => GetMonthlyOccurrences(rule.StartDate, start, end),
            RecurringCadence.Yearly => GetYearlyOccurrences(rule.StartDate, start, end),
            _ => [],
        };
    }

    private static IReadOnlyList<DateOnly> GetWeeklyOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int intervalDays
    )
    {
        var daysSinceAnchor = start.DayNumber - anchor.DayNumber;
        var daysUntilOccurrence = ((intervalDays - (daysSinceAnchor % intervalDays)) % intervalDays);
        var occurrence = start.AddDays(daysUntilOccurrence);
        var occurrences = new List<DateOnly>();

        while (occurrence <= end)
        {
            occurrences.Add(occurrence);
            occurrence = occurrence.AddDays(intervalDays);
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetMonthlyOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end
    )
    {
        var occurrences = new List<DateOnly>();
        var month = new DateOnly(start.Year, start.Month, 1);
        var lastMonth = new DateOnly(end.Year, end.Month, 1);

        while (month <= lastMonth)
        {
            var day = Math.Min(anchor.Day, DateTime.DaysInMonth(month.Year, month.Month));
            var occurrence = new DateOnly(month.Year, month.Month, day);
            if (occurrence >= start && occurrence <= end)
            {
                occurrences.Add(occurrence);
            }
            month = month.AddMonths(1);
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetYearlyOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end
    )
    {
        var occurrences = new List<DateOnly>();
        for (var year = start.Year; year <= end.Year; year++)
        {
            var day = Math.Min(anchor.Day, DateTime.DaysInMonth(year, anchor.Month));
            var occurrence = new DateOnly(year, anchor.Month, day);
            if (occurrence >= start && occurrence <= end)
            {
                occurrences.Add(occurrence);
            }
        }

        return occurrences;
    }

    private static DateOnly Max(DateOnly first, DateOnly second) =>
        first > second ? first : second;

    private static DateOnly Min(DateOnly first, DateOnly second) =>
        first < second ? first : second;
}