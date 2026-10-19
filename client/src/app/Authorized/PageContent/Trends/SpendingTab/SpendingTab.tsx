import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { MonthToolcards } from "@teelur/budget-board-ui";
import { getMonthToolcardStatus } from "~/helpers/monthToolcards";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import { Stack } from "@mantine/core";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const SpendingTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { dayjs, dayjsLocale } = useLocale();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().subtract(1, "month").startOf("month").toDate(),
    dayjs().startOf("month").toDate(),
  ]);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const timeToMonthlyTotalsMap = React.useMemo(
    () =>
      buildTimeToMonthlyTotalsMap(selectedMonths, transactionsQuery.data ?? []),
    [selectedMonths, transactionsQuery.data],
  );

  return (
    <Stack p="0.5rem">
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        getMonthStatus={(date) =>
          getMonthToolcardStatus(timeToMonthlyTotalsMap, date)
        }
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
        locale={dayjsLocale}
      />
      <SelectLastNMonths
        monthButtons={monthButtons}
        selectedMonths={selectedMonths}
        setSelectedMonths={setSelectedMonths}
        size="compact-sm"
        showClearButton
      />
      <SpendingChart
        h={400}
        transactions={transactionsQuery.data ?? []}
        months={selectedMonths}
        isPending={transactionsQuery.isPending}
        includeGrid
        includeYAxis
      />
    </Stack>
  );
};

export default SpendingTab;
