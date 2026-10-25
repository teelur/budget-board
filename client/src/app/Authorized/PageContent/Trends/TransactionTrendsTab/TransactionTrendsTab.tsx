import { Group, Stack } from "@mantine/core";
import {
  CategoryMultiSelect,
  MonthToolcards,
  Select,
} from "@teelur/budget-board-ui";
import TransactionTrendsChart from "~/components/Charts/TransactionTrendsChart/TransactionTrendsChart";
import { getMonthToolcardStatus } from "~/helpers/monthToolcards";
import {
  buildTimeToMonthlyTotalsMap,
  filterTransactionsByCategories,
  filterTransactionsByType,
} from "~/helpers/transactions";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import { CategoryTypes, type CategoryType } from "~/models/category";

const TransactionTrendsTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { t } = useTranslation();
  const { dayjs, dayjsLocale } = useLocale();
  const {
    allTransactionCategories,
    getCategoryType,
    isPending: categoriesPending,
  } = useTransactionCategories();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().subtract(1, "month").startOf("month").toDate(),
    dayjs().startOf("month").toDate(),
  ]);
  const [selectedCategoryValues, setSelectedCategoryValues] = React.useState<
    string[]
  >([]);
  const [selectedTransactionType, setSelectedTransactionType] =
    React.useState<CategoryType>(CategoryTypes.Expense);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const filteredTransactions = React.useMemo(() => {
    const transactions = transactionsQuery.data ?? [];
    return selectedCategoryValues.length > 0
      ? filterTransactionsByCategories(
          transactions,
          selectedCategoryValues,
          allTransactionCategories,
        )
      : filterTransactionsByType(
          transactions,
          selectedTransactionType,
          getCategoryType,
        );
  }, [
    allTransactionCategories,
    getCategoryType,
    selectedCategoryValues,
    selectedTransactionType,
    transactionsQuery.data,
  ]);

  const timeToMonthlyTotalsMap = React.useMemo(
    () =>
      buildTimeToMonthlyTotalsMap(selectedMonths, transactionsQuery.data ?? []),
    [selectedMonths, transactionsQuery.data],
  );

  return (
    <Stack p="0.5rem">
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        getMonthStatus={(date) =>
          getMonthToolcardStatus(timeToMonthlyTotalsMap, date)
        }
        isPending={transactionsQuery.isPending || categoriesPending}
        allowSelectMultiple
        locale={dayjsLocale}
      />
      <Group w="100%" justify="space-between" align="flex-end" wrap="wrap">
        <CategoryMultiSelect
          label={t("categories")}
          categories={allTransactionCategories}
          miw={220}
          value={selectedCategoryValues}
          onChange={setSelectedCategoryValues}
        />
        {selectedCategoryValues.length === 0 && (
          <Select
            label={t("transaction_type")}
            data={[
              { value: CategoryTypes.Expense, label: t("spending") },
              { value: CategoryTypes.Income, label: t("income") },
            ]}
            value={selectedTransactionType}
            onChange={(value) => {
              if (
                value === CategoryTypes.Expense ||
                value === CategoryTypes.Income
              ) {
                setSelectedTransactionType(value);
              }
            }}
            miw={160}
          />
        )}
        <SelectLastNMonths
          monthButtons={monthButtons}
          selectedMonths={selectedMonths}
          setSelectedMonths={setSelectedMonths}
          size="compact-sm"
          showClearButton
        />
      </Group>
      <TransactionTrendsChart
        height={400}
        transactions={filteredTransactions}
        months={selectedMonths}
        isPending={transactionsQuery.isPending || categoriesPending}
        includeGrid
        includeYAxis
        getCategoryType={
          selectedCategoryValues.length > 0 ||
          selectedTransactionType === CategoryTypes.Income
            ? getCategoryType
            : undefined
        }
      />
    </Stack>
  );
};

export default TransactionTrendsTab;
