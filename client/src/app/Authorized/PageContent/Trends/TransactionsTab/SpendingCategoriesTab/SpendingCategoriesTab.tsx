import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import { Button, Flex, Group, Stack } from "@mantine/core";
import React from "react";
import SpendingCategoriesChart from "~/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTranslation } from "react-i18next";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { CategoryTypes } from "~/models/category";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const SpendingCategoriesTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const [showSubcategories, setShowSubcategories] = React.useState(true);

  const selectedMonthKeys = new Set(
    selectedMonths.map((m) => `${m.getMonth()}-${m.getUTCFullYear()}`),
  );
  const transactionsForSelectedMonths =
    selectedMonths.length > 0
      ? (transactionsQuery.data ?? []).filter((t) => {
          const d = new Date(t.date);
          return selectedMonthKeys.has(`${d.getMonth()}-${d.getUTCFullYear()}`);
        })
      : (transactionsQuery.data ?? []);

  const expenseCategoryValues = new Set(
    transactionCategories
      .filter(
        (c) => c.parent === "" && c.categoryType === CategoryTypes.Expense,
      )
      .map((c) => c.value.toLowerCase()),
  );

  const expenseTransactions = transactionsForSelectedMonths.filter(
    (tx) =>
      tx.category == null ||
      tx.category === "" ||
      expenseCategoryValues.has(tx.category.toLowerCase()),
  );

  return (
    <Stack p="0.5rem" gap="1rem">
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsQuery.data ?? [],
        )}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
      <Group w="100%" justify="space-between">
        <Button
          size="compact-sm"
          variant="subtle"
          onClick={() => setShowSubcategories((v) => !v)}
        >
          {showSubcategories
            ? t("hide_subcategories")
            : t("show_subcategories")}
        </Button>
        <Group gap="xs">
          {monthButtons.map((months) => (
            <Button
              key={months}
              size="compact-sm"
              variant="light"
              onClick={() => {
                const newMonths: Date[] = [];
                for (let i = 0; i < months; i++) {
                  newMonths.push(
                    dayjs().subtract(i, "month").startOf("month").toDate(),
                  );
                }
                setSelectedMonths(newMonths);
              }}
            >
              {t("last_n_months", { count: months })}
            </Button>
          ))}
          <Button
            size="compact-sm"
            variant="primary"
            onClick={() => setSelectedMonths([])}
          >
            {t("clear_selection")}
          </Button>
        </Group>
      </Group>
      <Flex justify="center">
        <SpendingCategoriesChart
          transactions={expenseTransactions}
          categories={transactionCategories}
          showSubcategories={showSubcategories}
          isPending={transactionsQuery.isPending}
        />
      </Flex>
    </Stack>
  );
};

export default SpendingCategoriesTab;
