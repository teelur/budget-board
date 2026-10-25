export const GRID_COLS = 12;
export const GRID_ROW_HEIGHT = 20;
export const GRID_BREAKPOINT = 768;

export const TRANSACTION_TRENDS_WIDGET_TYPE = "TransactionTrends";
export const LEGACY_SPENDING_TRENDS_WIDGET_TYPE = "SpendingTrends";

// Spending trends was depricated and replaced by transaction trends
export const isTransactionTrendsWidgetType = (widgetType: string): boolean =>
  widgetType === TRANSACTION_TRENDS_WIDGET_TYPE ||
  widgetType === LEGACY_SPENDING_TRENDS_WIDGET_TYPE;

export interface WidgetRegistryEntry {
  widgetType: string;
  labelKey: string;
  descriptionKey: string;
  maxInstances: number;
}

export const WIDGET_REGISTRY: WidgetRegistryEntry[] = [
  {
    widgetType: "Accounts",
    labelKey: "accounts",
    descriptionKey: "accounts_widget_description",
    maxInstances: Infinity,
  },
  {
    widgetType: "NetWorth",
    labelKey: "net_worth",
    descriptionKey: "net_worth_widget_description",
    maxInstances: Infinity,
  },
  {
    widgetType: "UncategorizedTransactions",
    labelKey: "uncategorized_transactions",
    descriptionKey: "uncategorized_transactions_widget_description",
    maxInstances: 1,
  },
  {
    widgetType: TRANSACTION_TRENDS_WIDGET_TYPE,
    labelKey: "transaction_trends",
    descriptionKey: "transaction_trends_widget_description",
    maxInstances: 1,
  },
  {
    widgetType: "Flows",
    labelKey: "flows",
    descriptionKey: "flows_widget_description",
    maxInstances: 1,
  },
  {
    widgetType: "Metric",
    labelKey: "metric",
    descriptionKey: "metric_widget_description",
    maxInstances: Infinity,
  },
];
