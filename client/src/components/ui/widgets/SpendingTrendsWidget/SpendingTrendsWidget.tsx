import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { getRollingTotalSpendingForMonth } from "~/helpers/transactions";
import { Box, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LineChartIcon } from "lucide-react";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const SpendingTrendsWidget = (): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

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
          <PrimaryHeading order={3} lh={1}>
            {t("spending_trends")}
          </PrimaryHeading>
        </Group>
      }
      elevation={1}
    >
      <Stack gap={0} w="100%" p={"0.5rem"} style={{ flex: 1, minHeight: 0 }}>
        {transactionsQuery.isPending ? (
          <Skeleton height="100%" radius="md" />
        ) : (
          <>
            <DimmedText size="sm" ta="right">
              {getSpendingComparisonString()}
            </DimmedText>
            <Box style={{ flex: 1, minHeight: 0 }}>
              <SpendingChart
                months={months}
                transactions={transactionsQuery.data ?? []}
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
