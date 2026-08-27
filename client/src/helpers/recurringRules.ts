import { IRecurringForecastOccurrence } from "~/models/recurringRule";

export const buildCategoryToRecurringForecastTotalMap = (
  occurrences: IRecurringForecastOccurrence[],
): Map<string, number> => {
  const totals = new Map<string, number>();

  occurrences.forEach((occurrence) => {
    const category = (occurrence.category ?? "").toLocaleLowerCase();
    totals.set(category, (totals.get(category) ?? 0) + occurrence.amount);

    const subcategory = (occurrence.subcategory ?? "").toLocaleLowerCase();
    if (subcategory && subcategory !== category) {
      totals.set(subcategory, (totals.get(subcategory) ?? 0) + occurrence.amount);
    }
  });

  return totals;
};