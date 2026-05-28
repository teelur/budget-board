export interface IMetricWidgetConfiguration {
  markup: string;
}

export interface LiteralToken {
  type: "literal";
  text: string;
}

export interface ExpressionToken {
  type: "expression";
  source: string;
  metric: string;
  period?: string;
  params: Record<string, string>;
  format: string;
  raw: string;
}

export type MetricToken = LiteralToken | ExpressionToken;

export interface ParsedMetricMarkup {
  title?: MetricToken[];
  value?: MetricToken[];
  label?: MetricToken[];
}

export interface MetricDataRequirements {
  needsTransactions: boolean;
  transactionMonths: Date[];
  needsAllTimeTransactions: boolean;
  needsBudgets: boolean;
  budgetMonths: Date[];
  needsGoals: boolean;
  needsAccounts: boolean;
}
