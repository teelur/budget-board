import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo, getDaysInMonth } from "~/helpers/datetime";
import {
  filterHiddenTransactions,
  getRollingTotalSpendingForMonth,
} from "~/helpers/transactions";
import { Skeleton, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQueries, useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

const SpendingTrendsCard = (): React.ReactNode => {
  const months = [getDateFromMonthsAgo(0), getDateFromMonthsAgo(1)];

  const { t } = useTranslation();
  const { dayjs } = useDate();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

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
    const today = dayjs().date();

    const thisMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) =>
            dayjs(transaction.date).month() === dayjs(months[0])?.month(),
        ),
      ),
      today,
    );
    const lastMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) =>
            dayjs(transaction.date).month() === dayjs(months[1])?.month(),
        ),
      ),
      getDaysInMonth(
        dayjs(months[1])?.month() ?? 0,
        dayjs(months[1])?.year() ?? 0,
      ),
    );

    if (
      today >
      getDaysInMonth(
        dayjs(months[1])?.month() ?? 0,
        dayjs(months[1])?.year() ?? 0,
      )
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

    const amount = convertNumberToCurrency(
      Math.abs(spendingComparisonNumber),
      true,
      userSettingsQuery.data?.currency ?? "USD",
    );

    if (spendingComparisonNumber < 0) {
      if (userSettingsQuery.isPending) {
        return "";
      }
      return t("spending_trends_less_than_last_month", { amount });
    } else if (spendingComparisonNumber > 0) {
      if (userSettingsQuery.isPending) {
        return "";
      }
      return t("spending_trends_more_than_last_month", { amount });
    }

    return t("spending_trends_same_as_last_month");
  };

  return (
    <Card w="100%" elevation={1}>
      <Stack align="center" gap={0}>
        <PrimaryText size="xl">{t("spending_trends")}</PrimaryText>
        <DimmedText size="sm">{getSpendingComparisonString()}</DimmedText>
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
