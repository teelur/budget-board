import { getStandardDate, getUniqueDates } from "~/helpers/datetime";
import { chartColors } from "~/helpers/charts";
import dayjs from "dayjs";

export interface IValue {
  id: string;
  dateTime: Date;
  amount: number;
  parentId: string;
}

export interface IItem {
  id: string;
  name: string;
}

interface ValueChartData {
  date: Date;
  dateString: string;
  [key: string]: number | Date | string;
}

/**
 * Builds the series for the value chart.
 * @param items
 * @returns An array of objects containing the item ID, name, and color to use.
 */
export const buildValueChartSeries = (items: IItem[]) =>
  items.map((item: IItem) => {
    return {
      name: item.id,
      label: item.name,
      color: chartColors[items.indexOf(item) % chartColors.length] ?? "gray.6",
    };
  });

/**
 * Generates a sorted array of Date objects from an array of IValues entries.
 * @param values - Collection of value objects.
 * @returns Sorted list of Date objects in ascending order.
 */
export const getSortedValueDates = (values: IValue[]): Date[] =>
  values
    .map((value) => getStandardDate(dayjs(value.dateTime).toDate()))
    .sort((a, b) => a.getTime() - b.getTime());

/**
 * Builds data for a value chart based on provided values and date range.
 *
 * @param values An array of IValue objects representing values.
 * @param startDate The start date for the chart data.
 * @param endDate The end date for the chart data.
 * @param invertData Optional. If true, inverts the value data (e.g., for representing expenses as negative values). Defaults to false.
 * @returns An array of objects, where each object represents a date and the corresponding value for each item on that date.
 */
export const buildValueChartData = (
  sortedValues: IValue[],
  formatDateString: (date: Date) => string,
  invertData = false,
): ValueChartData[] => {
  const itemIdToSortedValuesMap = Map.groupBy(
    sortedValues,
    (value: IValue) => value.parentId,
  );

  // When multiple accounts are selected, some dates might not be represented on all accounts.
  // We need to aggregate all dates that have an associated balance for at least one account.
  const distinctSortedValueDates: Date[] = getUniqueDates(
    getSortedValueDates(sortedValues),
  );

  const chartData: ValueChartData[] = [];

  distinctSortedValueDates.forEach((date: Date, index: number) => {
    const dateString = formatDateString(date);

    const chartDataPoint: ValueChartData = { date, dateString };

    itemIdToSortedValuesMap.forEach((itemValues, itemId) => {
      const valuesForDate = itemValues.filter(
        (value) =>
          getStandardDate(new Date(value.dateTime)).getTime() ===
          getStandardDate(date).getTime(),
      );

      if (valuesForDate.length > 0) {
        chartDataPoint[itemId] =
          (valuesForDate.reduce((acc, v) => acc + v.amount, 0) /
            valuesForDate.length) *
          (invertData ? -1 : 1);
      } else if (index > 0) {
        chartDataPoint[itemId] = chartData[index - 1]![itemId]!;
      } else {
        chartDataPoint[itemId] = 0;
      }
    });

    chartData.push(chartDataPoint);
  });

  return chartData;
};

/**
 * Filters a collection of value entries by a specified date range.
 *
 * @param values - A list of value entries
 * @param startDate - The start date of the range
 * @param endDate - The end date of the range
 * @returns A new array of value entries within the specified range
 */
export const filterValuesByDateRange = (
  values: IValue[],
  startDate: Date,
  endDate: Date,
): IValue[] =>
  values.filter((value) =>
    dayjs(value.dateTime).isBetween(startDate, endDate, "date", "[]"),
  );
