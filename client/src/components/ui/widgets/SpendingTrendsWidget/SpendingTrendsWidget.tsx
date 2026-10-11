import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { SignDisplay } from "~/helpers/currency";
import { getRollingTotalSpendingForMonth } from "~/helpers/transactions";
import { Box, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { LineChartIcon } from "lucide-react";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { BodyText, Card, HeadingText } from "@teelur/budget-board-ui";

const SpendingTrendsWidget = (): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const months = [
    dayjs().startOf("month").toDate(),
    dayjs().subtract(1, "month").startOf("month").toDate(),
  ];
  const transactionsQuery = useTransactionsQuery({
    selectedDates: months.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const getSpendingComparison = (): number => {
    const today = dayjs().date();

    const thisMonthNum = months[0]?.getMonth();
    const lastMonthNum = months[1]?.getMonth();
    const daysInLastMonth = dayjs(months[1]).daysInMonth();

    const thisMonthRollingTotal = getRollingTotalSpendingForMonth(
      (transactionsQuery.data ?? []).filter(
        (transaction) => dayjs(transaction.date).month() === thisMonthNum,
      ),
      today,
    );
    const lastMonthRollingTotal = getRollingTotalSpendingForMonth(
      (transactionsQuery.data ?? []).filter(
        (transaction) => dayjs(transaction.date).month() === lastMonthNum,
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
    const spendingComparisonNumber = getSpendingComparison();

    const amount = formatSensitiveAmount(
      Math.abs(spendingComparisonNumber),
      SignDisplay.Auto,
    );

    if (spendingComparisonNumber < 0) {
      return t("spending_trends_less_than_last_month", { amount });
    } else if (spendingComparisonNumber > 0) {
      return t("spending_trends_more_than_last_month", { amount });
    }

    return t("spending_trends_same_as_last_month");
  };

  return (
    <Card h="100%" w="100%">
      <Card.Section separatorThickness="thick">
        <Group gap="0.25rem">
          <LineChartIcon size="1.5rem" color="var(--bbui-color-primary)" />
          <HeadingText level={4}>{t("spending_trends")}</HeadingText>
        </Group>
      </Card.Section>
      <Card.Section
        style={{
          display: "flex",
          flex: "1 1 0",
          flexDirection: "column",
          minHeight: 0,
        }}
      >
        {transactionsQuery.isPending ? (
          <Skeleton height="100%" radius="md" />
        ) : (
          <Stack gap={0} style={{ flex: "1 1 0", minHeight: 0 }}>
            <BodyText tone="secondary" size="sm" ta="right">
              {getSpendingComparisonString()}
            </BodyText>
            <Box style={{ flex: "1 1 0", minHeight: 0 }}>
              <SpendingChart
                months={months}
                transactions={transactionsQuery.data ?? []}
                includeYAxis={false}
              />
            </Box>
          </Stack>
        )}
      </Card.Section>
    </Card>
  );
};

export default SpendingTrendsWidget;
