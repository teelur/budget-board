import { LayoutItem } from "react-grid-layout";

export const GRID_COLS = 12;
export const GRID_ROW_HEIGHT = 80;
export const GRID_BREAKPOINT = 768;

export interface WidgetRegistryEntry {
  widgetType: string;
  labelKey: string;
  descriptionKey: string;
  maxInstances: number;
  defaultW: number;
  defaultH: number;
}

export const WIDGET_REGISTRY: WidgetRegistryEntry[] = [
  {
    widgetType: "Accounts",
    labelKey: "accounts",
    descriptionKey: "accounts_widget_description",
    maxInstances: 1,
    defaultW: 4,
    defaultH: 5,
  },
  {
    widgetType: "NetWorth",
    labelKey: "net_worth",
    descriptionKey: "net_worth_widget_description",
    maxInstances: Infinity,
    defaultW: 4,
    defaultH: 5,
  },
  {
    widgetType: "UncategorizedTransactions",
    labelKey: "uncategorized_transactions",
    descriptionKey: "uncategorized_transactions_widget_description",
    maxInstances: 1,
    defaultW: 8,
    defaultH: 5,
  },
  {
    widgetType: "SpendingTrends",
    labelKey: "spending_trends",
    descriptionKey: "spending_trends_widget_description",
    maxInstances: Infinity,
    defaultW: 8,
    defaultH: 5,
  },
];

/**
 * Derives a single-column mobile layout from the desktop layout.
 * Items are sorted by y then x and stacked vertically.
 */
export const deriveSmLayout = (lgLayout: LayoutItem[]): LayoutItem[] => {
  const sorted = lgLayout.slice().sort((a, b) => {
    if (a.y !== b.y) return a.y - b.y;
    return a.x - b.x;
  });

  let currentY = 0;
  return sorted.map((item) => {
    const smItem: LayoutItem = { ...item, x: 0, w: 1, y: currentY };
    currentY += item.h;
    return smItem;
  });
};
