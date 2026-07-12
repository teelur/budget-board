import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import { Stack } from "@mantine/core";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const SpendingTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { dayjs } = useLocale();

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

  return (
    <Stack p={"0.5rem"}>
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsQuery.data ?? [],
        )}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
      <SelectLastNMonths
        monthButtons={monthButtons}
        setSelectedMonths={setSelectedMonths}
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
