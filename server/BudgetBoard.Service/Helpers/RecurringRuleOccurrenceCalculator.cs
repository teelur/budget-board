using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

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

        var cadence = RecurringCadenceSerializer.Deserialize(rule.Cadence);
        return GetOccurrences(rule.StartDate, start, end, cadence);
    }

    internal static IReadOnlyList<DateOnly> GetOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        RecurringCadence cadence
    )
    {
        var isPerUnit = cadence.Mode == RecurringCadenceModeValues.PerUnit;
        var occurrences = cadence.Unit switch
        {
            RecurringCadenceUnitValues.Day => GetDayOccurrences(
                anchor,
                start,
                end,
                isPerUnit ? 1 : cadence.Interval
            ),
            RecurringCadenceUnitValues.Week => isPerUnit
                ? GetWeeklyPerUnitOccurrences(anchor, start, end, cadence.Interval)
                : GetDayOccurrences(anchor, start, end, (long)cadence.Interval * 7),
            RecurringCadenceUnitValues.Month => isPerUnit
                ? GetMonthlyPerUnitOccurrences(anchor, start, end, cadence.Interval)
                : GetMonthlyOccurrences(anchor, start, end, cadence.Interval),
            RecurringCadenceUnitValues.Year => isPerUnit
                ? GetYearlyPerUnitOccurrences(anchor, start, end, cadence.Interval)
                : GetYearlyOccurrences(anchor, start, end, cadence.Interval),
            _ => throw new RecurringCadenceValidationException("Cadence unit is not supported."),
        };

        return occurrences.Distinct().OrderBy(date => date).ToList();
    }

    private static IReadOnlyList<DateOnly> GetWeeklyPerUnitOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int occurrencesPerWeek
    )
    {
        var firstWeekIndex = (start.DayNumber - anchor.DayNumber) / 7;
        var weekStartDay = (long)anchor.DayNumber + firstWeekIndex * 7L;
        var occurrences = new List<DateOnly>();

        while (weekStartDay <= end.DayNumber)
        {
            for (var occurrenceIndex = 0; occurrenceIndex < occurrencesPerWeek; occurrenceIndex++)
            {
                var occurrenceDay = weekStartDay + occurrenceIndex * 7L / occurrencesPerWeek;
                if (occurrenceDay >= start.DayNumber && occurrenceDay <= end.DayNumber)
                {
                    occurrences.Add(DateOnly.FromDayNumber((int)occurrenceDay));
                }
            }

            weekStartDay += 7;
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetMonthlyPerUnitOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int occurrencesPerMonth
    )
    {
        var occurrences = new List<DateOnly>();
        var monthIndex = GetMonthIndex(start);
        var endMonthIndex = GetMonthIndex(end);

        while (monthIndex <= endMonthIndex)
        {
            var month = GetDateFromMonthIndex(monthIndex);
            var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            var anchorDay = Math.Min(anchor.Day, daysInMonth) - 1;

            for (var occurrenceIndex = 0; occurrenceIndex < occurrencesPerMonth; occurrenceIndex++)
            {
                var day =
                    (anchorDay + occurrenceIndex * daysInMonth / occurrencesPerMonth) % daysInMonth
                    + 1;
                var occurrence = new DateOnly(month.Year, month.Month, day);
                if (occurrence >= start && occurrence <= end)
                {
                    occurrences.Add(occurrence);
                }
            }

            monthIndex++;
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetYearlyPerUnitOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int occurrencesPerYear
    )
    {
        var occurrences = new List<DateOnly>();
        for (var year = start.Year; year <= end.Year; year++)
        {
            var daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
            var anchorDate = new DateOnly(
                year,
                anchor.Month,
                Math.Min(anchor.Day, DateTime.DaysInMonth(year, anchor.Month))
            );
            var anchorDay = anchorDate.DayOfYear - 1;

            for (var occurrenceIndex = 0; occurrenceIndex < occurrencesPerYear; occurrenceIndex++)
            {
                var dayOfYear =
                    (anchorDay + occurrenceIndex * daysInYear / occurrencesPerYear) % daysInYear;
                var occurrence = new DateOnly(year, 1, 1).AddDays(dayOfYear);
                if (occurrence >= start && occurrence <= end)
                {
                    occurrences.Add(occurrence);
                }
            }
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetDayOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        long intervalDays
    )
    {
        var daysSinceAnchor = (long)start.DayNumber - anchor.DayNumber;
        var daysUntilOccurrence = (intervalDays - (daysSinceAnchor % intervalDays)) % intervalDays;
        var occurrenceDay = (long)start.DayNumber + daysUntilOccurrence;
        var occurrences = new List<DateOnly>();

        while (occurrenceDay <= end.DayNumber)
        {
            occurrences.Add(DateOnly.FromDayNumber((int)occurrenceDay));
            occurrenceDay += intervalDays;
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetMonthlyOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int intervalMonths
    )
    {
        var occurrences = new List<DateOnly>();
        var anchorMonth = GetMonthIndex(anchor);
        var startMonth = GetMonthIndex(start);
        var endMonth = GetMonthIndex(end);
        var monthsSinceAnchor = startMonth - anchorMonth;
        var monthsUntilOccurrence =
            (intervalMonths - (monthsSinceAnchor % intervalMonths)) % intervalMonths;
        var monthIndex = startMonth + monthsUntilOccurrence;

        while (monthIndex <= endMonth)
        {
            var month = GetDateFromMonthIndex(monthIndex);
            var day = Math.Min(anchor.Day, DateTime.DaysInMonth(month.Year, month.Month));
            var occurrence = new DateOnly(month.Year, month.Month, day);
            if (occurrence >= start && occurrence <= end)
            {
                occurrences.Add(occurrence);
            }
            monthIndex += intervalMonths;
        }

        return occurrences;
    }

    private static IReadOnlyList<DateOnly> GetYearlyOccurrences(
        DateOnly anchor,
        DateOnly start,
        DateOnly end,
        int intervalYears
    )
    {
        var occurrences = new List<DateOnly>();
        var yearsSinceAnchor = start.Year - anchor.Year;
        var yearsUntilOccurrence =
            (intervalYears - (yearsSinceAnchor % intervalYears)) % intervalYears;
        var year = (long)start.Year + yearsUntilOccurrence;
        while (year <= end.Year)
        {
            var day = Math.Min(anchor.Day, DateTime.DaysInMonth((int)year, anchor.Month));
            var occurrence = new DateOnly((int)year, anchor.Month, day);
            if (occurrence >= start && occurrence <= end)
            {
                occurrences.Add(occurrence);
            }
            year += intervalYears;
        }

        return occurrences;
    }

    private static long GetMonthIndex(DateOnly date) => (long)date.Year * 12 + date.Month - 1;

    private static DateOnly GetDateFromMonthIndex(long monthIndex)
    {
        var year = (int)(monthIndex / 12);
        var month = (int)(monthIndex % 12 + 1);
        return new DateOnly(year, month, 1);
    }

    private static DateOnly Max(DateOnly first, DateOnly second) => first > second ? first : second;

    private static DateOnly Min(DateOnly first, DateOnly second) => first < second ? first : second;
}
