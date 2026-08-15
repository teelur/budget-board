import React from "react";
import { Stack } from "@mantine/core";
import FlowsChart from "~/components/Charts/FlowsChart/FlowsChart";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

const FlowsTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];
  const { dayjs } = useLocale();
  const { allTransactionCategories } = useTransactionCategories();
  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  return (
    <Stack p="0.5rem">
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
      <FlowsChart
        transactions={transactionsQuery.data ?? []}
        categories={allTransactionCategories}
        isPending={transactionsQuery.isPending}
      />
    </Stack>
  );
};

export default FlowsTab;
