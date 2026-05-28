export const GRID_COLS = 12;
export const GRID_ROW_HEIGHT = 20;
export const GRID_BREAKPOINT = 768;

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
    maxInstances: 1,
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
    widgetType: "SpendingTrends",
    labelKey: "spending_trends",
    descriptionKey: "spending_trends_widget_description",
    maxInstances: Infinity,
  },
  {
    widgetType: "Metric",
    labelKey: "metric",
    descriptionKey: "metric_widget_description",
    maxInstances: Infinity,
  },
];
