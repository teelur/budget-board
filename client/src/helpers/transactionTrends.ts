export type TransactionTrendsComparisonType =
  "categories" | "income" | "spending";

const comparisonKeys = {
  categories: {
    less: "transaction_trends_selected_categories_less_than_last_month",
    more: "transaction_trends_selected_categories_more_than_last_month",
    same: "transaction_trends_selected_categories_same_as_last_month",
  },
  income: {
    less: "transaction_trends_income_less_than_last_month",
    more: "transaction_trends_income_more_than_last_month",
    same: "transaction_trends_income_same_as_last_month",
  },
  spending: {
    less: "transaction_trends_spending_less_than_last_month",
    more: "transaction_trends_spending_more_than_last_month",
    same: "transaction_trends_spending_same_as_last_month",
  },
} as const;

export const getTransactionTrendsComparisonKey = (
  comparisonType: TransactionTrendsComparisonType,
  difference: number,
) => {
  const keys = comparisonKeys[comparisonType];
  return difference < 0 ? keys.less : difference > 0 ? keys.more : keys.same;
};
