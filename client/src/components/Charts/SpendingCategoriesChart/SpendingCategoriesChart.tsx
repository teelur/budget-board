import {
  SpendingCategoriesChart as BBSpendingCategoriesChart,
  type SpendingCategoryChartData,
} from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import {
  buildSpendingCategoryChartData,
  buildSpendingSubcategoryChartData,
} from "~/helpers/charts";
import { SignDisplay } from "~/helpers/currency";
import {
  hiddenTransactionCategory,
  ITransaction,
  uncategorizedTransactionCategory,
} from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { areStringsEqual } from "~/helpers/utils";
import { CategoryTypes } from "~/models/category";

interface SpendingChartProps {
  transactions: ITransaction[];
  isPending?: boolean;
  showSubcategories?: boolean;
}

const SpendingCategoriesChart = ({
  transactions,
  isPending,
  showSubcategories,
}: SpendingChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories, getCategoryType } =
    useTransactionCategories();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const filteredTransactions = React.useMemo(
    () =>
      transactions.filter(
        (tx) =>
          tx.amount < 0 &&
          !areStringsEqual(tx.category, hiddenTransactionCategory) &&
          areStringsEqual(
            getCategoryType(tx.category ?? ""),
            CategoryTypes.Expense,
          ),
      ),
    [transactions, getCategoryType],
  );

  const data = React.useMemo<SpendingCategoryChartData[]>(() => {
    const translateName = (name: string) =>
      name.toLowerCase() === uncategorizedTransactionCategory
        ? t("uncategorized")
        : name;
    const categories = buildSpendingCategoryChartData(
      filteredTransactions,
      allTransactionCategories,
    );
    const subcategories = buildSpendingSubcategoryChartData(
      filteredTransactions,
      allTransactionCategories,
      categories,
    );

    return categories.map((category) => ({
      name: translateName(category.name),
      value: category.value,
      subcategories: subcategories
        .filter((subcategory) => subcategory.parent === category.name)
        .map((subcategory) => ({
          name: translateName(subcategory.name),
          value: subcategory.value,
        })),
    }));
  }, [allTransactionCategories, filteredTransactions, t]);

  const formatValue = React.useCallback(
    (value: number) => formatSensitiveAmount(value, SignDisplay.Auto),
    [formatSensitiveAmount],
  );

  return (
    <BBSpendingCategoriesChart
      ariaLabel={t("spending_categories")}
      data={data}
      emptyMessage={t("select_a_month_to_display_the_chart")}
      formatValue={formatValue}
      isLoading={isPending}
      showSubcategories={showSubcategories}
      totalLabel={t("total_spending")}
    />
  );
};

export default SpendingCategoriesChart;
