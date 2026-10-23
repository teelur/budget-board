import type { MonthToolcardStatus } from "@teelur/budget-board-ui";
import { getCashFlowValue } from "~/helpers/budgets";
import { CashFlowValue } from "~/models/budget";

export const getMonthToolcardStatus = (
  timeToMonthlyTotalsMap: Map<number, number>,
  date: Date,
): MonthToolcardStatus => {
  const cashFlowValue = getCashFlowValue(timeToMonthlyTotalsMap, date);

  if (cashFlowValue === CashFlowValue.Positive) {
    return "positive";
  }

  if (cashFlowValue === CashFlowValue.Negative) {
    return "negative";
  }

  return "neutral";
};
