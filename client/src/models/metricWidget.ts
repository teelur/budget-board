export interface LiteralToken {
  type: "literal";
  text: string;
}

export interface MetricDateRangeExpression {
  start: string;
  end: string;
}

export interface ExpressionToken {
  type: "expression";
  source: string;
  metric: string;
  period?: string;
  range?: MetricDateRangeExpression;
  periodError?: string;
  params: Record<string, string>;
  raw: string;
}

export type MetricToken = LiteralToken | ExpressionToken;

export interface MetricDataRequirements {
  needsTransactions: boolean;
  transactionMonths: Date[];
  needsAllTimeTransactions: boolean;
  needsBudgets: boolean;
  budgetMonths: Date[];
  needsGoals: boolean;
  needsAccounts: boolean;
}
