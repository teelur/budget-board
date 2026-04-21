import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import {
  filterHiddenTransactions,
  getRollingTotalSpendingForMonth,
} from "~/helpers/transactions";
import { Box, Group, Skeleton, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LineChartIcon } from "lucide-react";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

const SpendingTrendsWidget = (): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { request } = useAuth();
  const { preferredCurrency } = useUserSettings();

  const months = [
    dayjs().startOf("month").toDate(),
    dayjs().subtract(1, "month").startOf("month").toDate(),
  ];
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

  const getSpendingComparison = (): number => {
    const today = dayjs().date();

    const thisMonthNum = months[0]?.getMonth();
    const lastMonthNum = months[1]?.getMonth();
    const daysInLastMonth = dayjs(months[1]).daysInMonth();

    const thisMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) => dayjs(transaction.date).month() === thisMonthNum,
        ),
      ),
      today,
    );
    const lastMonthRollingTotal = getRollingTotalSpendingForMonth(
      filterHiddenTransactions(
        (transactionsQueries.data ?? []).filter(
          (transaction) => dayjs(transaction.date).month() === lastMonthNum,
        ),
      ),
      daysInLastMonth,
    );

    if (today > daysInLastMonth) {
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
      preferredCurrency,
      SignDisplay.Auto,
      intlLocale,
    );

    if (spendingComparisonNumber < 0) {
      return t("spending_trends_less_than_last_month", { amount });
    } else if (spendingComparisonNumber > 0) {
      return t("spending_trends_more_than_last_month", { amount });
    }

    return t("spending_trends_same_as_last_month");
  };

  return (
    <SplitCard
      w="100%"
      h="100%"
      border={BorderThickness.Thick}
      header={
        <Group gap="0.25rem">
          <LineChartIcon color="var(--base-color-text-dimmed)" />
          <PrimaryText size="xl" lh={1}>
            {t("spending_trends")}
          </PrimaryText>
        </Group>
      }
      elevation={1}
    >
      <Stack gap={0} w="100%" style={{ flex: 1, minHeight: 0 }}>
        {transactionsQueries.isPending ? (
          <Skeleton height="100%" radius="md" />
        ) : (
          <>
            <DimmedText size="sm" ta="right">
              {getSpendingComparisonString()}
            </DimmedText>
            <Box style={{ flex: 1, minHeight: 0 }}>
              <SpendingChart
                months={months}
                transactions={filterHiddenTransactions(
                  transactionsQueries.data ?? [],
                )}
                includeYAxis={false}
              />
            </Box>
          </>
        )}
      </Stack>
    </SplitCard>
  );
};

export default SpendingTrendsWidget;
