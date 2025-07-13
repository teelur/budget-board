import classes from "./Budgets.module.css";

import { Stack } from "@mantine/core";
import React from "react";
import BudgetsToolbar from "./BudgetsToolbar/BudgetsToolbar";
import { initCurrentMonth } from "~/helpers/datetime";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
} from "~/helpers/transactions";
import { useQueries, useQuery } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IBudget } from "~/models/budget";
import { AxiosResponse } from "axios";
import {
  defaultTransactionCategories,
  ITransaction,
} from "~/models/transaction";
import { ICategoryResponse } from "~/models/category";
import BudgetsContent from "./BudgetsContent/BudgetsContent";

const Budgets = (): React.ReactNode => {
  const [selectedDates, setSelectedDates] = React.useState<Date[]>([
    initCurrentMonth(),
  ]);

  const { request } = React.useContext<any>(AuthContext);

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

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

  // We need to filter out the transactions labelled with 'Hide From Budgets'
  const transactionsWithoutHidden = filterHiddenTransactions(
    transactionsForMonthsQuery.data ?? []
  );

  const timeToMonthlyTotalsMap: Map<number, number> = React.useMemo(
    () => buildTimeToMonthlyTotalsMap(selectedDates, transactionsWithoutHidden),
    [selectedDates, transactionsWithoutHidden]
  );

  return (
    <Stack className={classes.root}>
      <BudgetsToolbar
        categories={transactionCategoriesWithCustom}
        selectedDates={selectedDates}
        setSelectedDates={setSelectedDates}
        timeToMonthlyTotalsMap={timeToMonthlyTotalsMap}
        showCopy={
          !budgetsQuery.isPending &&
          budgetsQuery.data.length === 0 &&
          selectedDates.length === 1
        }
        isPending={
          budgetsQuery.isPending || transactionCategoriesQuery.isPending
        }
      />
      <BudgetsContent
        budgets={budgetsQuery.data ?? []}
        categories={transactionCategoriesWithCustom}
        transactions={transactionsWithoutHidden}
        selectedDate={
          selectedDates.length === 1 ? selectedDates[0] ?? null : null
        }
        isPending={
          budgetsQuery.isPending || transactionCategoriesQuery.isPending
        }
      />
    </Stack>
  );
};

export default Budgets;
