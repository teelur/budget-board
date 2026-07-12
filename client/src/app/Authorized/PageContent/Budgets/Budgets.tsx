import { Stack } from "@mantine/core";
import React from "react";
import BudgetsToolbar from "./BudgetsToolbar/BudgetsToolbar";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import BudgetsContent from "./BudgetsContent/BudgetsContent";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const Budgets = (): React.ReactNode => {
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const { dayjs } = useLocale();

  const [selectedDates, setSelectedDates] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);

  const budgetsQuery = useBudgetsQuery({
    months: selectedDates,
    enabled: selectedDates.length > 0,
  });

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedDates.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const timeToMonthlyTotalsMap: Map<number, number> = React.useMemo(
    () =>
      buildTimeToMonthlyTotalsMap(selectedDates, transactionsQuery.data ?? []),
    [selectedDates, transactionsQuery.data],
  );

  return (
    <Stack w="100%" maw={1400}>
      <BudgetsToolbar
        categories={transactionCategories}
        selectedDates={selectedDates}
        setSelectedDates={setSelectedDates}
        timeToMonthlyTotalsMap={timeToMonthlyTotalsMap}
        showCopy={
          !budgetsQuery.isPending &&
          budgetsQuery.data.length === 0 &&
          selectedDates.length === 1
        }
        isPending={budgetsQuery.isPending}
      />
      <BudgetsContent
        budgets={budgetsQuery.data ?? []}
        categories={transactionCategories}
        transactions={transactionsQuery.data ?? []}
        selectedDate={
          selectedDates.length === 1 ? (selectedDates[0] ?? null) : null
        }
        isPending={budgetsQuery.isPending}
      />
    </Stack>
  );
};

export default Budgets;
