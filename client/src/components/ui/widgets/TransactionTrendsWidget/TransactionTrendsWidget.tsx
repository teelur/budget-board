import { Box, Group, Stack } from "@mantine/core";
import {
  Alert,
  BodyText,
  Card,
  HeadingText,
  Skeleton,
} from "@teelur/budget-board-ui";
import TransactionTrendsChart from "~/components/Charts/TransactionTrendsChart/TransactionTrendsChart";
import { SignDisplay } from "~/helpers/currency";
import React from "react";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { LineChartIcon } from "lucide-react";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import {
  filterTransactionsByCategories,
  filterTransactionsByType,
  getRollingTransactionTotalForMonth,
} from "~/helpers/transactions";
import { getTransactionTrendsComparisonKey } from "~/helpers/transactionTrends";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { parseTransactionTrendsConfiguration } from "~/helpers/widgets";
import TransactionTrendsWidgetSettings from "./TransactionTrendsWidgetSettings/TransactionTrendsWidgetSettings";
import { CategoryTypes } from "~/models/category";
import { useRecurringForecastQuery } from "~/hooks/queries/useRecurringForecastQuery";
import {
  filterRecurringForecastByCategories,
  filterRecurringForecastByType,
} from "~/helpers/recurringRules";

interface TransactionTrendsWidgetProps {
  widget: IWidgetSettingsResponse;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const TransactionTrendsWidget = ({
  widget,
  settingsOpened,
  onSettingsClose,
}: TransactionTrendsWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const formatSensitiveAmount = useSensitiveAmountFormatter();
  const {
    allTransactionCategories,
    getCategoryType,
    isPending: categoriesPending,
  } = useTransactionCategories();

  const months = [
    dayjs().startOf("month").toDate(),
    dayjs().subtract(1, "month").startOf("month").toDate(),
  ];
  const { categoryValues, transactionType, showProjections } = React.useMemo(
    () => parseTransactionTrendsConfiguration(widget.configuration),
    [widget.configuration],
  );
  const transactionsQuery = useTransactionsQuery({
    selectedDates: months.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });
  const forecastQuery = useRecurringForecastQuery({
    month: dayjs().startOf("month").toDate(),
    enabled: showProjections,
  });
  const filteredTransactions = React.useMemo(() => {
    const transactions = transactionsQuery.data ?? [];
    return categoryValues.length > 0
      ? filterTransactionsByCategories(
          transactions,
          categoryValues,
          allTransactionCategories,
        )
      : filterTransactionsByType(
          transactions,
          transactionType,
          getCategoryType,
        );
  }, [
    allTransactionCategories,
    categoryValues,
    getCategoryType,
    transactionType,
    transactionsQuery.data,
  ]);
  const filteredProjectedOccurrences = React.useMemo(() => {
    const occurrences = forecastQuery.data ?? [];
    return categoryValues.length > 0
      ? filterRecurringForecastByCategories(
          occurrences,
          categoryValues,
          allTransactionCategories,
        )
      : filterRecurringForecastByType(
          occurrences,
          transactionType,
          getCategoryType,
        );
  }, [
    allTransactionCategories,
    categoryValues,
    forecastQuery.data,
    getCategoryType,
    transactionType,
  ]);
  const isPending =
    transactionsQuery.isPending ||
    categoriesPending ||
    (showProjections && forecastQuery.isPending);

  const comparisonType =
    categoryValues.length > 0
      ? "categories"
      : transactionType === CategoryTypes.Income
        ? "income"
        : "spending";

  const getTransactionComparison = (): number => {
    const today = dayjs().date();

    const thisMonthNum = months[0]?.getMonth();
    const lastMonthNum = months[1]?.getMonth();
    const daysInLastMonth = dayjs(months[1]).daysInMonth();

    const thisMonthRollingTotal = getRollingTransactionTotalForMonth(
      filteredTransactions.filter(
        (transaction) => dayjs(transaction.date).month() === thisMonthNum,
      ),
      today,
      categoryValues.length > 0 || transactionType === CategoryTypes.Income
        ? getCategoryType
        : undefined,
    );
    const lastMonthRollingTotal = getRollingTransactionTotalForMonth(
      filteredTransactions.filter(
        (transaction) => dayjs(transaction.date).month() === lastMonthNum,
      ),
      daysInLastMonth,
      categoryValues.length > 0 || transactionType === CategoryTypes.Income
        ? getCategoryType
        : undefined,
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

  const getTransactionComparisonString = (): string => {
    const transactionComparisonNumber = getTransactionComparison();
    const amount = formatSensitiveAmount(
      Math.abs(transactionComparisonNumber),
      SignDisplay.Auto,
    );

    return t(
      getTransactionTrendsComparisonKey(
        comparisonType,
        transactionComparisonNumber,
      ),
      { amount },
    );
  };

  return (
    <>
      <Card h="100%" w="100%">
        <Card.Section separatorThickness="thick">
          <Group gap="0.25rem">
            <LineChartIcon size="1.5rem" color="var(--bbui-color-primary)" />
            <HeadingText level={4}>{t("transaction_trends")}</HeadingText>
            {categoryValues.length > 0 && (
              <BodyText tone="secondary" size="xs" ta="right">
                {t("categories")}: {categoryValues.join(", ")}
              </BodyText>
            )}
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
          {isPending ? (
            <Skeleton height="100%" />
          ) : (
            <Stack gap={0} style={{ flex: "1 1 0", minHeight: 0 }}>
              {showProjections && forecastQuery.isError && (
                <Alert color="error" variant="light">
                  {t("transaction_trends_projections_load_error")}
                </Alert>
              )}
              <BodyText tone="primary" size="sm" ta="right">
                {getTransactionComparisonString()}
              </BodyText>
              <Box style={{ flex: "1 1 0", minHeight: 0 }}>
                <TransactionTrendsChart
                  months={months}
                  transactions={filteredTransactions}
                  projectedOccurrences={filteredProjectedOccurrences}
                  includeProjections={showProjections && !forecastQuery.isError}
                  includeYAxis={false}
                  getCategoryType={
                    categoryValues.length > 0 ||
                    transactionType === CategoryTypes.Income
                      ? getCategoryType
                      : undefined
                  }
                />
              </Box>
            </Stack>
          )}
        </Card.Section>
      </Card>
      {settingsOpened !== undefined && onSettingsClose && (
        <TransactionTrendsWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </>
  );
};

export default TransactionTrendsWidget;
