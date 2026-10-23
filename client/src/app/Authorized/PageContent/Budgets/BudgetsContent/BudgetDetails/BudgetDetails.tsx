import { Group, Stack } from "@mantine/core";
import {
  AmountText,
  BodyText,
  CollapsibleCard,
  Drawer,
  HeadingText,
  Skeleton,
} from "@teelur/budget-board-ui";
import React from "react";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import MonthlySpendingChart from "~/components/Charts/MonthlySpendingChart/MonthlySpendingChart";
import { getCategoryIcon, getIsParentCategory } from "~/helpers/category";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { areStringsEqual } from "~/helpers/utils";
import TransactionCards from "./TransactionCards/TransactionCards";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { CategoryTypes } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { useRecurringForecastQueries } from "~/hooks/queries/useRecurringForecastQueries";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface BudgetDetailsProps {
  isOpen: boolean;
  close: () => void;
  category: string | null;
  months: Date[];
}

const BudgetDetails = (props: BudgetDetailsProps): React.ReactNode => {
  const chartLookbackMonths = 6;

  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { allTransactionCategories, getCategoryType } =
    useTransactionCategories();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const transactionsQuery = useTransactionsQuery();
  const forecastQueries = useRecurringForecastQueries({
    months: props.months,
    enabled: props.isOpen && props.months.length > 0,
  });

  const categoryIcon = getCategoryIcon(
    props.category ?? "",
    allTransactionCategories,
  );

  const selectedMonths = [...props.months].sort(
    (a, b) => a.getTime() - b.getTime(),
  );
  const selectedMonthRanges: { start: Date; end: Date }[] = [];
  selectedMonths.forEach((month) => {
    const lastRange = selectedMonthRanges[selectedMonthRanges.length - 1];
    if (
      lastRange &&
      dayjs(lastRange.end).add(1, "month").isSame(month, "month")
    ) {
      lastRange.end = month;
    } else {
      selectedMonthRanges.push({ start: month, end: month });
    }
  });
  const monthHeader = selectedMonthRanges
    .map(({ start, end }) => {
      const formattedStart = dayjs(start).format("MMMM YYYY");
      return dayjs(start).isSame(end, "month")
        ? formattedStart
        : `${formattedStart} - ${dayjs(end).format("MMMM YYYY")}`;
    })
    .join(", ");
  const selectedMonthKeys = new Set(
    selectedMonths.map((month) => dayjs(month).format("YYYY-MM")),
  );
  const chartMonths =
    selectedMonths.length === 1
      ? Array.from({ length: chartLookbackMonths }, (_, i) =>
          getDateFromMonthsAgo(i, selectedMonths[0]),
        )
      : selectedMonths;
  const chartMonthKeys = new Set(
    chartMonths.map((month) => dayjs(month).format("YYYY-MM")),
  );

  const isTransactionForCategory = (transaction: ITransaction): boolean => {
    if (
      !props.category ||
      getIsParentCategory(props.category, allTransactionCategories)
    ) {
      return areStringsEqual(transaction.category ?? "", props.category ?? "");
    }
    return areStringsEqual(transaction.subcategory ?? "", props.category);
  };

  const transactionsForCategory = (transactionsQuery.data ?? [])
    .filter((transaction) =>
      chartMonthKeys.has(dayjs(transaction.date).format("YYYY-MM")),
    )
    .filter(isTransactionForCategory);

  const transactionsForCategoryForSelectedMonths = (
    transactionsQuery.data ?? []
  )
    .filter((transaction) =>
      selectedMonthKeys.has(dayjs(transaction.date).format("YYYY-MM")),
    )
    .filter(isTransactionForCategory);

  const forecastForCategory = (forecastQueries.data ?? [])
    .filter((occurrence) => {
      if (
        !props.category ||
        getIsParentCategory(props.category, allTransactionCategories)
      ) {
        return areStringsEqual(occurrence.category ?? "", props.category ?? "");
      }
      return areStringsEqual(occurrence.subcategory ?? "", props.category);
    })
    .sort((a, b) => dayjs(a.date).valueOf() - dayjs(b.date).valueOf());

  const isExpenseCategory = areStringsEqual(
    getCategoryType(props.category ?? ""),
    CategoryTypes.Expense,
  );

  const getContent = (): React.ReactNode => {
    if (transactionsQuery.isPending) {
      return <Skeleton height={425} />;
    }

    if (selectedMonths.length === 0 || !props.category) {
      return null;
    }

    return (
      <Stack gap="1rem">
        <Group justify="space-between" align="center">
          <Stack gap={0}>
            <BodyText tone="secondary" size="xs">
              {t("category")}
            </BodyText>
            <BodyText tone="primary" size="lg" fw={500}>
              {categoryIcon.length > 0 ? `${categoryIcon} ` : ""}
              {props.category ?? t("no_category")}
            </BodyText>
          </Stack>
          <Stack gap={0}>
            <BodyText tone="secondary" size="xs">
              {t("month")}
            </BodyText>
            <BodyText tone="primary" size="lg" fw={500}>
              {monthHeader}
            </BodyText>
          </Stack>
        </Group>
        <Stack gap="0.5rem">
          <CollapsibleCard
            title={
              <HeadingText tone="heading" level={5}>
                {isExpenseCategory ? t("expense_trends") : t("income_trends")}
              </HeadingText>
            }
          >
            <MonthlySpendingChart
              transactions={transactionsForCategory ?? []}
              months={chartMonths}
              includeYAxis={false}
              invertData={isExpenseCategory}
            />
          </CollapsibleCard>
          <CollapsibleCard
            title={
              <HeadingText tone="heading" level={5}>
                {t("recent_transactions")}
              </HeadingText>
            }
          >
            <TransactionCards
              transactions={transactionsForCategoryForSelectedMonths}
              categories={allTransactionCategories}
            />
          </CollapsibleCard>
          <CollapsibleCard
            title={
              <HeadingText tone="heading" level={5}>
                {t("projected")}
              </HeadingText>
            }
          >
            {forecastQueries.isPending ? (
              <Skeleton height={35} />
            ) : forecastForCategory.length > 0 ? (
              <Stack gap="0.25rem">
                {forecastForCategory.map((occurrence) => (
                  <Group
                    key={`${occurrence.ruleID}-${occurrence.date}`}
                    justify="space-between"
                  >
                    <BodyText tone="primary" size="sm" fw={500}>
                      {dayjs(occurrence.date).format("LL")}
                    </BodyText>
                    <AmountText
                      size="sm"
                      amount={occurrence.amount}
                      isSensitive={isPrivacyModeEnabled}
                      locale={intlLocale}
                      currency={preferredCurrency}
                      decimalPlaces={decimalPlaces}
                    />
                  </Group>
                ))}
              </Stack>
            ) : (
              <BodyText tone="muted" size="sm">
                {t("no_projected_transactions")}
              </BodyText>
            )}
          </CollapsibleCard>
        </Stack>
      </Stack>
    );
  };

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={t("budget_details")}
    >
      {getContent()}
    </Drawer>
  );
};

export default BudgetDetails;
