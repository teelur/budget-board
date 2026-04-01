import { Stack } from "@mantine/core";
import React from "react";
import BudgetsToolbar from "./BudgetsToolbar/BudgetsToolbar";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
} from "~/helpers/transactions";
import { useQueries } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IBudget } from "~/models/budget";
import { AxiosResponse } from "axios";
import { ITransaction } from "~/models/transaction";
import BudgetsContent from "./BudgetsContent/BudgetsContent";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

const Budgets = (): React.ReactNode => {
  const { transactionCategories } = useTransactionCategories();
  const { dayjs } = useLocale();
  const { request } = useAuth();

  const [selectedDates, setSelectedDates] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);

  const budgetsQuery = useQueries({
    queries: selectedDates.map((date: Date) => ({
      queryKey: ["budgets", date],
      queryFn: async (): Promise<IBudget[]> => {
        const res: AxiosResponse = await request({
          url: "/api/budget",
          method: "GET",
          params: { date },
        });

        if (res.status === 200) {
          return res.data as IBudget[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  const transactionsForMonthsQuery = useQueries({
    queries: selectedDates.map((date: Date) => ({
      queryKey: [
        "transactions",
        { month: date.getMonth(), year: date.getUTCFullYear() },
      ],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: { month: date.getMonth() + 1, year: date.getFullYear() },
        });

        if (res.status === 200) {
          return res.data as ITransaction[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  // We need to filter out the transactions labelled with 'Hide From Budgets'
  const transactionsWithoutHidden = filterHiddenTransactions(
    transactionsForMonthsQuery.data ?? [],
  );

  const timeToMonthlyTotalsMap: Map<number, number> = React.useMemo(
    () => buildTimeToMonthlyTotalsMap(selectedDates, transactionsWithoutHidden),
    [selectedDates, transactionsWithoutHidden],
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
        transactions={transactionsWithoutHidden}
        selectedDate={
          selectedDates.length === 1 ? (selectedDates[0] ?? null) : null
        }
        isPending={budgetsQuery.isPending}
      />
    </Stack>
  );
};

export default Budgets;
