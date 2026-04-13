import { DateString, getUniqueDates } from "~/helpers/datetime";
import { chartColors } from "~/helpers/charts";
import dayjs from "dayjs";

export interface IValue {
  id: string;
  date: DateString;
  amount: number;
  parentId: string;
}

export interface IItem {
  id: string;
  name: string;
}

interface ValueChartData {
  date: string;
  [key: string]: number | string;
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
  formatDateString: (date: DateString) => string,
  invertData = false,
): ValueChartData[] => {
  // When multiple items are selected, some dates might not be represented on all items.
  // We need to aggregate all dates that have an associated balance for at least one item.
  const distinctSortedValueDates: DateString[] = getUniqueDates(
    sortedValues
      .map((value) => value.date)
      .sort((a, b) => dayjs(a).diff(dayjs(b))),
  );

  const valuesByItemId = Map.groupBy(
    sortedValues,
    (value: IValue) => value.parentId,
  );
  const chartData: ValueChartData[] = [];

  distinctSortedValueDates.forEach((date: DateString, index: number) => {
    const formattedDateString = formatDateString(date);

    const chartDataPoint: ValueChartData = {
      date: formattedDateString,
    };

    valuesByItemId.forEach((itemValues, itemId) => {
      const valuesForDate = itemValues.filter((value) => value.date === date);

      if (valuesForDate.length > 0) {
        chartDataPoint[itemId] =
          valuesForDate[0]!.amount * (invertData ? -1 : 1);
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
