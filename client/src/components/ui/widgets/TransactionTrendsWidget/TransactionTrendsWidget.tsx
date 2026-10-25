import { Box, Group, Stack } from "@mantine/core";
import { BodyText, Card, HeadingText, Skeleton } from "@teelur/budget-board-ui";
import TransactionTrendsChart from "~/components/Charts/TransactionTrendsChart/TransactionTrendsChart";
import { SignDisplay } from "~/helpers/currency";
import { getRollingTransactionTotalForMonth } from "~/helpers/transactions";
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
} from "~/helpers/transactions";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { parseTransactionTrendsConfiguration } from "~/helpers/widgets";
import TransactionTrendsWidgetSettings from "./TransactionTrendsWidgetSettings/TransactionTrendsWidgetSettings";
import { CategoryTypes } from "~/models/category";

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
  const { categoryValues, transactionType } = React.useMemo(
    () => parseTransactionTrendsConfiguration(widget.configuration),
    [widget.configuration],
  );
  const transactionsQuery = useTransactionsQuery({
    selectedDates: months.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
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
  const isPending = transactionsQuery.isPending || categoriesPending;

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
    const categoryComparison = categoryValues.length > 0;
    const incomeComparison =
      !categoryComparison && transactionType === CategoryTypes.Income;

    const amount = formatSensitiveAmount(
      Math.abs(transactionComparisonNumber),
      SignDisplay.Auto,
    );

    if (transactionComparisonNumber < 0) {
      return t(
        categoryComparison
          ? "transaction_trends_selected_categories_less_than_last_month"
          : incomeComparison
            ? "transaction_trends_income_less_than_last_month"
            : "transaction_trends_spending_less_than_last_month",
        { amount },
      );
    } else if (transactionComparisonNumber > 0) {
      return t(
        categoryComparison
          ? "transaction_trends_selected_categories_more_than_last_month"
          : incomeComparison
            ? "transaction_trends_income_more_than_last_month"
            : "transaction_trends_spending_more_than_last_month",
        { amount },
      );
    }

    return t(
      categoryComparison
        ? "transaction_trends_selected_categories_same_as_last_month"
        : incomeComparison
          ? "transaction_trends_income_same_as_last_month"
          : "transaction_trends_spending_same_as_last_month",
    );
  };

  return (
    <>
      <Card h="100%" w="100%">
        <Card.Section separatorThickness="thick">
          <Group gap="0.25rem">
            <LineChartIcon size="1.5rem" color="var(--bbui-color-primary)" />
            <HeadingText level={4}>{t("transaction_trends")}</HeadingText>
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
              <BodyText tone="secondary" size="sm" ta="right">
                {getTransactionComparisonString()}
              </BodyText>
              <Box style={{ flex: "1 1 0", minHeight: 0 }}>
                <TransactionTrendsChart
                  months={months}
                  transactions={filteredTransactions}
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
