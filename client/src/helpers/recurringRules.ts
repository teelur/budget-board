import dayjs, { Dayjs } from "dayjs";
import { TFunction } from "i18next";
import {
  defaultRecurringCadence,
  IRecurringCadence,
  IRecurringForecastOccurrence,
  RecurringCadenceMode,
  RecurringCadenceModes,
  RecurringCadenceUnit,
  RecurringCadenceUnits,
} from "~/models/recurringRule";

const PREVIEW_OCCURRENCE_COUNT = 6;

const LEGACY_CADENCE_MAP: Record<string, IRecurringCadence> = {
  weekly: { version: 1, unit: RecurringCadenceUnits.Week, interval: 1 },
  biweekly: { version: 1, unit: RecurringCadenceUnits.Week, interval: 2 },
  monthly: { version: 1, unit: RecurringCadenceUnits.Month, interval: 1 },
  yearly: { version: 1, unit: RecurringCadenceUnits.Year, interval: 1 },
};

const createUnsupportedRecurringCadence = (): IRecurringCadence => ({
  ...defaultRecurringCadence,
  unsupported: true,
});

export const createRecurringCadence = (
  unit: RecurringCadenceUnit,
  interval: number,
  mode: RecurringCadenceMode = RecurringCadenceModes.Interval,
): IRecurringCadence => ({
  version: 1,
  unit,
  interval,
  ...(mode === RecurringCadenceModes.PerUnit ? { mode } : {}),
});

export const getRecurringCadenceIntervalMaximum = (
  unit: RecurringCadenceUnit,
  mode: RecurringCadenceMode,
): number | undefined => {
  if (mode !== RecurringCadenceModes.PerUnit) {
    return undefined;
  }

  switch (unit) {
    case RecurringCadenceUnits.Day:
      return 1;
    case RecurringCadenceUnits.Week:
      return 7;
    case RecurringCadenceUnits.Month:
      return 31;
    case RecurringCadenceUnits.Year:
      return 366;
  }
};

export const normalizeRecurringCadence = (
  cadence: unknown,
): IRecurringCadence => {
  if (typeof cadence === "string") {
    const legacyCadence = LEGACY_CADENCE_MAP[cadence.trim().toLowerCase()];
    if (legacyCadence) {
      return legacyCadence;
    }

    if (cadence.trim().startsWith("{")) {
      try {
        return normalizeRecurringCadence(JSON.parse(cadence));
      } catch {
        return createUnsupportedRecurringCadence();
      }
    }

    return createUnsupportedRecurringCadence();
  }

  if (typeof cadence !== "object" || cadence === null) {
    return createUnsupportedRecurringCadence();
  }

  const rawCadence = cadence as Record<string, unknown>;
  const version = Number(rawCadence.version ?? rawCadence.Version);
  const interval = Number(rawCadence.interval ?? rawCadence.Interval);
  const rawUnit = rawCadence.unit ?? rawCadence.Unit;
  const rawMode = rawCadence.mode ?? rawCadence.Mode;
  const unit =
    typeof rawUnit === "string"
      ? Object.values(RecurringCadenceUnits).find(
          (candidate) => candidate.toLowerCase() === rawUnit.toLowerCase(),
        )
      : undefined;
  let mode: RecurringCadenceMode = RecurringCadenceModes.Interval;
  if (typeof rawMode === "string") {
    const matchingMode = Object.values(RecurringCadenceModes).find(
      (candidate) => candidate.toLowerCase() === rawMode.toLowerCase(),
    );
    if (!matchingMode) {
      return createUnsupportedRecurringCadence();
    }
    mode = matchingMode;
  }
  const maximumInterval = unit
    ? getRecurringCadenceIntervalMaximum(unit, mode)
    : undefined;

  if (
    version === 1 &&
    unit &&
    Number.isInteger(interval) &&
    interval > 0 &&
    (maximumInterval === undefined || interval <= maximumInterval)
  ) {
    return createRecurringCadence(unit, interval, mode);
  }

  return createUnsupportedRecurringCadence();
};

export const getRecurringCadenceLabel = (
  cadence: unknown,
  translate: TFunction,
): string => {
  const normalizedCadence = normalizeRecurringCadence(cadence);
  if (normalizedCadence.unsupported) {
    return translate("recurring_cadence_unsupported");
  }

  const unitKey = normalizedCadence.unit.toLowerCase();
  if (normalizedCadence.mode === RecurringCadenceModes.PerUnit) {
    if (normalizedCadence.interval === 1) {
      return translate(`recurring_cadence_${unitKey}`);
    }

    return translate("recurring_cadence_per_unit", {
      count: normalizedCadence.interval,
      unit: translate(`recurring_unit_${unitKey}`),
    });
  }

  if (normalizedCadence.interval === 1) {
    return translate(`recurring_cadence_${unitKey}`);
  }

  return translate("recurring_cadence_interval", {
    count: normalizedCadence.interval,
    unit: translate(`recurring_unit_${unitKey}_plural`),
  });
};

export const getUpcomingRecurringDates = (
  cadence: IRecurringCadence,
  startDate: string,
  endDate?: string | null,
): string[] => {
  if (
    cadence.unsupported ||
    cadence.version !== 1 ||
    !Number.isInteger(cadence.interval) ||
    cadence.interval <= 0
  ) {
    return [];
  }

  const start = dayjs(startDate);
  const end = endDate ? dayjs(endDate) : start.add(1, "year");
  if (!start.isValid() || !end.isValid() || end.isBefore(start, "day")) {
    return [];
  }

  if (cadence.mode === RecurringCadenceModes.PerUnit) {
    const dates: string[] = [];
    const seenDates = new Set<string>();
    for (
      let periodIndex = 0;
      dates.length < PREVIEW_OCCURRENCE_COUNT;
      periodIndex++
    ) {
      const candidates = getPerUnitPeriodOccurrences(
        cadence,
        start,
        periodIndex,
      );
      candidates.forEach((candidate) => {
        if (
          dates.length < PREVIEW_OCCURRENCE_COUNT &&
          !candidate.isBefore(start, "day") &&
          !candidate.isAfter(end, "day") &&
          !seenDates.has(candidate.format("YYYY-MM-DD"))
        ) {
          const formattedDate = candidate.format("YYYY-MM-DD");
          seenDates.add(formattedDate);
          dates.push(formattedDate);
        }
      });

      const lastCandidate = candidates[candidates.length - 1];
      if (!lastCandidate || lastCandidate.isAfter(end, "day")) {
        break;
      }
    }
    return dates;
  }

  const dates: string[] = [];
  for (
    let occurrenceIndex = 0;
    dates.length < PREVIEW_OCCURRENCE_COUNT;
    occurrenceIndex++
  ) {
    const candidate = getRecurringOccurrence(cadence, start, occurrenceIndex);
    if (!candidate.isValid() || candidate.isAfter(end, "day")) {
      break;
    }
    dates.push(candidate.format("YYYY-MM-DD"));
  }

  return dates;
};

const getPerUnitPeriodOccurrences = (
  cadence: IRecurringCadence,
  start: Dayjs,
  periodIndex: number,
): Dayjs[] => {
  const occurrences = Array.from(
    { length: cadence.interval },
    (_, occurrenceIndex) => {
      switch (cadence.unit) {
        case RecurringCadenceUnits.Day:
          return start.add(periodIndex, "day");
        case RecurringCadenceUnits.Week:
          return start
            .add(periodIndex * 7, "day")
            .add(Math.floor((occurrenceIndex * 7) / cadence.interval), "day");
        case RecurringCadenceUnits.Month: {
          const month = start.add(periodIndex, "month").startOf("month");
          const daysInMonth = month.daysInMonth();
          const anchorDay = Math.min(start.date(), daysInMonth) - 1;
          const day =
            ((anchorDay +
              Math.floor((occurrenceIndex * daysInMonth) / cadence.interval)) %
              daysInMonth) +
            1;
          return month.date(day);
        }
        case RecurringCadenceUnits.Year: {
          const yearStart = start.add(periodIndex, "year").startOf("year");
          const year = yearStart.year();
          const daysInYear =
            year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0)
              ? 366
              : 365;
          const anchorDate = start
            .add(periodIndex, "year")
            .month(start.month())
            .date(
              Math.min(
                start.date(),
                start.add(periodIndex, "year").daysInMonth(),
              ),
            );
          const anchorDay = anchorDate.diff(yearStart, "day");
          const dayOfYear =
            (anchorDay +
              Math.floor((occurrenceIndex * daysInYear) / cadence.interval)) %
            daysInYear;
          return yearStart.add(dayOfYear, "day");
        }
      }
    },
  );

  return occurrences.sort(
    (first, second) => first.valueOf() - second.valueOf(),
  );
};

const getRecurringOccurrence = (
  cadence: IRecurringCadence,
  start: Dayjs,
  occurrenceIndex: number,
): Dayjs => {
  switch (cadence.unit) {
    case RecurringCadenceUnits.Day:
      return start.add(occurrenceIndex * cadence.interval, "day");
    case RecurringCadenceUnits.Week:
      return start.add(occurrenceIndex * cadence.interval * 7, "day");
    case RecurringCadenceUnits.Month: {
      const monthIndex =
        start.year() * 12 + start.month() + occurrenceIndex * cadence.interval;
      const year = Math.floor(monthIndex / 12);
      const month = monthIndex % 12;
      const firstOfMonth = dayjs(new Date(year, month, 1));
      return firstOfMonth.date(
        Math.min(start.date(), firstOfMonth.daysInMonth()),
      );
    }
    case RecurringCadenceUnits.Year: {
      const year = start.year() + occurrenceIndex * cadence.interval;
      const candidate = dayjs(new Date(year, start.month(), 1));
      return candidate.date(Math.min(start.date(), candidate.daysInMonth()));
    }
  }
};

export const buildCategoryToRecurringForecastTotalMap = (
  occurrences: IRecurringForecastOccurrence[],
): Map<string, number> => {
  const totals = new Map<string, number>();

  occurrences.forEach((occurrence) => {
    const category = (occurrence.category ?? "").toLocaleLowerCase();
    totals.set(category, (totals.get(category) ?? 0) + occurrence.amount);

    const subcategory = (occurrence.subcategory ?? "").toLocaleLowerCase();
    if (subcategory && subcategory !== category) {
      totals.set(
        subcategory,
        (totals.get(subcategory) ?? 0) + occurrence.amount,
      );
    }
  });

  return totals;
};
