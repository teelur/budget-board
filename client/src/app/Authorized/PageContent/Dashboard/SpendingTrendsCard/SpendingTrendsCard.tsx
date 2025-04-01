import classes from "./SpendingTrendsCard.module.css";

import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo, getDaysInMonth } from "~/helpers/datetime";
import {
  filterHiddenTransactions,
  getRollingTotalSpendingForMonth,
} from "~/helpers/transactions";
import { Card, Skeleton, Stack, Text, Title } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";

const SpendingTrendsCard = (): React.ReactNode => {
  const months = [getDateFromMonthsAgo(0), getDateFromMonthsAgo(1)];

  const { request } = React.useContext<any>(AuthContext);
  const transactionsQueries = useQueries({
    queries: months.map((date) => ({
      queryKey: [
        "transactions",
        { month: date.getMonth(), year: date.getFullYear() },
      ],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: {
            month: date.getMonth() + 1,
            year: date.getFullYear(),
          },
        });

        if (res.status === 200) {
          return res.data as ITransaction[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  if (transactionsQueries.isPending) {
    return <Skeleton height={500} radius="lg" />;
  }

  const getSpendingComparison = (): number => {
    const today = new Date().getDate();

    const thisMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) =>
            new Date(transaction.date).getMonth() === months[0]?.getMonth()
        )
      ),
      today
    );
    const lastMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) =>
            new Date(transaction.date).getMonth() === months[1]?.getMonth()
        )
      ),
      getDaysInMonth(months[1]?.getMonth() ?? 0, months[1]?.getFullYear() ?? 0)
    );

    if (
      today >
      getDaysInMonth(months[1]?.getMonth() ?? 0, months[1]?.getFullYear() ?? 0)
    ) {
      // If today is greater than the last day of the last month, we need to compare to the
      // last day of the last month.
      return (
        (thisMonthRollingTotal.at(today - 1)?.amount ?? 0) -
        (lastMonthRollingTotal.at(-1)?.amount ?? 0)
      );
    }

    return (
      (thisMonthRollingTotal.at(today - 1)?.amount ?? 0) -
      (lastMonthRollingTotal.at(today - 1)?.amount ?? 0)
    );
  };

  const getSpendingComparisonString = (): string => {
    // Need to round this number to the nearest cent
    const spendingComparisonNumber =
      Math.round((getSpendingComparison() + Number.EPSILON) * 100) / 100;

    if (spendingComparisonNumber < 0) {
      return `${convertNumberToCurrency(
        Math.abs(spendingComparisonNumber),
        true
      )} less than`;
    } else if (spendingComparisonNumber > 0) {
      return `${convertNumberToCurrency(
        Math.abs(spendingComparisonNumber),
        true
      )} more than`;
    }

    return "the same as";
  };

  return (
    <Card className={classes.root} withBorder radius="md">
      <Stack className={classes.header}>
        <Title>Spending Trends</Title>
        <Text>You have spent {getSpendingComparisonString()} last month.</Text>
      </Stack>
      <SpendingChart
        months={months}
        transactions={filterHiddenTransactions(transactionsQueries.data ?? [])}
        includeYAxis={false}
      />
    </Card>
  );
};

export default SpendingTrendsCard;
