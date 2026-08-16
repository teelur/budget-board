import {
  hiddenTransactionCategory,
  ITransaction,
  uncategorizedTransactionCategory,
} from "~/models/transaction";
import {
  getRollingTotalSpendingForMonth,
  getTransactionsForMonth,
  RollingTotalSpendingPerDay,
} from "./transactions";
import { getDaysInMonth } from "./datetime";
import { areStringsEqual } from "./utils";
import { getFormattedCategoryValue, getParentCategory } from "./category";
import { ICategory } from "~/models/category";

export const chartColors = [
  "indigo.6",
  "teal.6",
  "orange.6",
  "red.6",
  "yellow.6",
  "lime.6",
  "grape.6",
  "pink.6",
];

/**
 * Builds a dataset showing the cumulative spending trend for the given months.
 *
 * @param months - An array of Date objects representing each month to include.
 * @param transactions - A collection of transaction objects containing spending information.
 * @returns An array of objects containing day-by-day spending data across months.
 */
export const buildTransactionChartData = (
  months: Date[],
  transactions: ITransaction[],
  formatDateString: (date: Date) => string,
): any[] => {
  const spendingTrendsChartData: any[] = [];
  months.forEach((month) => {
    const transactionsForMonth = getTransactionsForMonth(transactions, month);

    // If it is the current month, we only want to show the data up to today's date.
    const today = new Date();
    const isThisMonth =
      month.getMonth() === today.getMonth() &&
      month.getFullYear() === today.getFullYear();

    const rollingTotalTransactionsForMonth: RollingTotalSpendingPerDay[] =
      getRollingTotalSpendingForMonth(
        transactionsForMonth,
        isThisMonth
          ? today.getDate()
          : getDaysInMonth(month.getMonth(), month.getFullYear()),
      );

    rollingTotalTransactionsForMonth.forEach(
      (rollingTotalTransaction: RollingTotalSpendingPerDay) => {
        const chartDay = spendingTrendsChartData.find(
          (data) => data.day === rollingTotalTransaction.day,
        );

        // On the very first loop, we need to create the data point.
        if (chartDay == null) {
          const newChartDay: any = {
            day: rollingTotalTransaction.day,
            [formatDateString(month)]: rollingTotalTransaction.amount,
          };
          spendingTrendsChartData.push(newChartDay);
        } else {
          chartDay[formatDateString(month)] = rollingTotalTransaction.amount;
        }
      },
    );
  });
  return spendingTrendsChartData;
};

/**
 * Builds the series for the transaction chart.
 *
 * @param months - An array of Date objects representing each month to include.
 * @returns An array of objects containing the name of the month and the color to use.
 */
export const buildTransactionChartSeries = (
  months: Date[],
  formatDateString: (date: Date) => string,
): { name: string; color: string }[] =>
  months.map((month, i) => ({
    name: formatDateString(month),
    color: chartColors[i % chartColors.length] ?? "gray.6",
  }));

/**
 * Builds chart data for spending categories based on a list of transactions and categories.
 *
 * Iterates through each transaction, determines its formatted category name,
 * and aggregates the transaction amounts by category. The result is an array
 * of objects, each representing a category and the total amount spent in that category.
 *
 * @param transactions - An array of transaction objects to be aggregated.
 * @param categories - An array of category objects used to format and match transaction categories.
 * @returns An array of objects, each containing a `name` (category) and `value` (total amount spent).
 */
export const buildSpendingCategoryChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
) => {
  const filteredTransactions = transactions.filter(
    (transaction) =>
      !areStringsEqual(transaction.category ?? "", hiddenTransactionCategory),
  );

  const totalsMap = new Map<string, number>();

  filteredTransactions.forEach((transaction) => {
    const formattedTransactionCategory = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    totalsMap.set(
      formattedTransactionCategory,
      (totalsMap.get(formattedTransactionCategory) ?? 0) +
        transaction.amount * -1,
    );
  });

  return Array.from(totalsMap.entries()).map(([name, value], i) => ({
    name,
    value,
    color: chartColors[i % chartColors.length] ?? "gray.6",
  }));
};

/**
 * Builds subcategory-level chart data for the outer ring of a two-ring pie chart.
 * Entries are ordered to align with the inner ring (grouped by parent).
 * Color shades are derived from the parent's color family.
 */
export const buildSpendingSubcategoryChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
  innerChartData: { name: string; color: string }[],
): any[] => {
  const filteredTransactions = transactions.filter(
    (transaction) =>
      !areStringsEqual(transaction.category ?? "", hiddenTransactionCategory),
  );

  const subMap = new Map<
    string,
    { name: string; value: number; parent: string }
  >();

  filteredTransactions.forEach((transaction) => {
    const parentName = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    const subName = transaction.subcategory
      ? getFormattedCategoryValue(transaction.subcategory, categories)
      : parentName;
    const key = `${parentName}::${subName}`;
    const existing = subMap.get(key);
    if (existing) {
      existing.value += transaction.amount * -1;
    } else {
      subMap.set(key, {
        name: subName,
        value: transaction.amount * -1,
        parent: parentName,
      });
    }
  });

  const shadeSteps = [4, 7, 3, 8, 2, 9, 5];
  const result: any[] = [];

  innerChartData.forEach((parent) => {
    const colorFamily = parent.color.split(".")[0] ?? "gray";
    const subs = [...subMap.values()].filter((s) => s.parent === parent.name);
    subs.forEach((sub, i) => {
      const shade =
        subs.length === 1 ? 6 : (shadeSteps[i % shadeSteps.length] ?? 6);
      result.push({
        name: sub.name,
        value: sub.value,
        color: `${colorFamily}.${shade}`,
        parent: sub.parent,
      });
    });
  });

  return result;
};

export type FlowType = "income" | "expense" | "surplus" | "deficit";

export interface FlowsChartNode {
  name: string;
  color?: string;
  flowType?: FlowType;
  parentName?: string;
  transactionCount?: number;
  [key: string]: unknown;
}

export interface FlowsChartLink {
  source: number;
  target: number;
  value: number;
  transactionCount?: number;
  flowType?: FlowType;
  [key: string]: unknown;
}

export interface FlowsChartData {
  nodes: FlowsChartNode[];
  links: FlowsChartLink[];
  totalIncome: number;
  totalSpending: number;
}

interface FlowsChartLabels {
  cashAvailable: string;
  surplus: string;
  deficit: string;
  total: string;
  income: string;
  expense: string;
  uncategorized: string;
}

/**
 * Builds an aggregated income-to-expense flow for the selected transactions.
 * The cash node is an accounting bridge and does not imply direct funding provenance.
 */
export const buildFlowsChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
  labels: FlowsChartLabels,
): FlowsChartData => {
  const nodeNames: string[] = [];
  const nodeIndexes = new Map<string, number>();
  const links = new Map<
    string,
    {
      source: string;
      target: string;
      value: number;
      transactionCount: number;
      flowType?: FlowType;
    }
  >();
  const nodeLayers = new Map<string, number>();
  const nodeGroups = new Map<string, string>();
  const nodeMetadata = new Map<
    string,
    { flowType?: FlowType; parentName?: string; transactionCount?: number }
  >();
  const transferCategoryName =
    categories.find(
      (category) =>
        category.parent === "" && areStringsEqual(category.value, "Transfer"),
    )?.value ?? "Transfer";
  let incomeTotal = 0;
  let expenseTotal = 0;
  const transferChildNets = new Map<string, number>();
  const transferChildCounts = new Map<string, number>();
  const categoryFlowNets = new Map<
    string,
    {
      parentName: string;
      leafNets: Map<string, number>;
      leafCounts: Map<string, number>;
    }
  >();

  const addNode = (name: string): string => {
    if (!nodeIndexes.has(name)) {
      nodeIndexes.set(name, nodeNames.length);
      nodeNames.push(name);
    }
    return name;
  };

  const addLink = (
    source: string,
    target: string,
    value: number,
    transactionCount = 0,
    flowType?: FlowType,
  ) => {
    if (source === target || value <= 0) {
      return;
    }

    addNode(source);
    addNode(target);
    const key = `${source}\u0000${target}`;
    const existing = links.get(key);
    if (existing) {
      existing.value += value;
      existing.transactionCount += transactionCount;
    } else {
      links.set(key, { source, target, value, transactionCount, flowType });
    }
  };

  const markNode = (
    name: string,
    layer: number,
    group?: string,
    metadata?: {
      flowType?: FlowType;
      parentName?: string;
      transactionCount?: number;
    },
  ) => {
    addNode(name);
    nodeLayers.set(name, layer);
    nodeGroups.set(name, group ?? name);
    if (metadata) {
      const existing = nodeMetadata.get(name) ?? {};
      const transactionCount =
        metadata.transactionCount !== undefined
          ? (existing.transactionCount ?? 0) + metadata.transactionCount
          : existing.transactionCount;

      nodeMetadata.set(name, { ...existing, ...metadata, transactionCount });
    }
  };

  transactions.forEach((transaction) => {
    if (
      transaction.deleted !== null ||
      areStringsEqual(transaction.category ?? "", hiddenTransactionCategory)
    ) {
      return;
    }

    const rawCategoryName = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    const rawSubcategoryName = transaction.subcategory
      ? getFormattedCategoryValue(transaction.subcategory, categories)
      : rawCategoryName;
    const rawParentName =
      getParentCategory(rawCategoryName, categories) || rawCategoryName;
    const categoryName = areStringsEqual(
      rawCategoryName,
      uncategorizedTransactionCategory,
    )
      ? labels.uncategorized
      : rawCategoryName;
    const subcategoryName = areStringsEqual(
      rawSubcategoryName,
      uncategorizedTransactionCategory,
    )
      ? labels.uncategorized
      : rawSubcategoryName;
    const parentName = areStringsEqual(
      rawParentName,
      uncategorizedTransactionCategory,
    )
      ? labels.uncategorized
      : rawParentName;
    const isTransfer =
      areStringsEqual(rawCategoryName, transferCategoryName) ||
      areStringsEqual(rawParentName, transferCategoryName) ||
      areStringsEqual(
        getParentCategory(rawSubcategoryName, categories),
        transferCategoryName,
      );

    if (isTransfer) {
      const transferChildName = transaction.subcategory
        ? subcategoryName
        : categoryName;
      transferChildNets.set(
        transferChildName,
        (transferChildNets.get(transferChildName) ?? 0) + transaction.amount,
      );
      transferChildCounts.set(
        transferChildName,
        (transferChildCounts.get(transferChildName) ?? 0) + 1,
      );
      return;
    }
    const leafName = transaction.subcategory
      ? subcategoryName
      : rawCategoryName === rawParentName
        ? `${categoryName} (${labels.total})`
        : categoryName;
    if (transaction.amount === 0) {
      return;
    }

    const categoryFlow = categoryFlowNets.get(parentName) ?? {
      parentName,
      leafNets: new Map<string, number>(),
      leafCounts: new Map<string, number>(),
    };
    categoryFlow.leafNets.set(
      leafName,
      (categoryFlow.leafNets.get(leafName) ?? 0) + transaction.amount,
    );
    categoryFlow.leafCounts.set(
      leafName,
      (categoryFlow.leafCounts.get(leafName) ?? 0) + 1,
    );
    categoryFlowNets.set(parentName, categoryFlow);
  });

  categoryFlowNets.forEach(({ parentName, leafNets, leafCounts }) => {
    const positiveChildren = [...leafNets.entries()].filter(
      ([, net]) => net > 0,
    );
    const negativeChildren = [...leafNets.entries()].filter(
      ([, net]) => net < 0,
    );

    if (positiveChildren.length > 0) {
      const branchName =
        negativeChildren.length > 0
          ? `${parentName} (${labels.income})`
          : parentName;
      const branchTotal = positiveChildren.reduce(
        (total, [, net]) => total + net,
        0,
      );
      const branchTransactionCount = positiveChildren.reduce(
        (total, [leafName]) => total + (leafCounts.get(leafName) ?? 0),
        0,
      );

      incomeTotal += branchTotal;
      markNode(branchName, 1, undefined, {
        flowType: "income",
        transactionCount: branchTransactionCount,
      });
      markNode(labels.cashAvailable, 2);
      positiveChildren.forEach(([leafName, net]) => {
        const transactionCount = leafCounts.get(leafName) ?? 0;
        markNode(leafName, 0, branchName, {
          flowType: "income",
          parentName: branchName,
          transactionCount,
        });
        addLink(leafName, branchName, net, transactionCount, "income");
      });
      addLink(
        branchName,
        labels.cashAvailable,
        branchTotal,
        branchTransactionCount,
        "income",
      );
    }

    if (negativeChildren.length > 0) {
      const branchName =
        positiveChildren.length > 0
          ? `${parentName} (${labels.expense})`
          : parentName;
      const branchTotal = negativeChildren.reduce(
        (total, [, net]) => total + Math.abs(net),
        0,
      );
      const branchTransactionCount = negativeChildren.reduce(
        (total, [leafName]) => total + (leafCounts.get(leafName) ?? 0),
        0,
      );

      expenseTotal += branchTotal;
      markNode(labels.cashAvailable, 2);
      markNode(branchName, 3, undefined, {
        flowType: "expense",
        transactionCount: branchTransactionCount,
      });
      negativeChildren.forEach(([leafName, net]) => {
        const transactionCount = leafCounts.get(leafName) ?? 0;
        markNode(leafName, 4, branchName, {
          flowType: "expense",
          parentName: branchName,
          transactionCount,
        });
        addLink(
          branchName,
          leafName,
          Math.abs(net),
          transactionCount,
          "expense",
        );
      });
      addLink(
        labels.cashAvailable,
        branchName,
        branchTotal,
        branchTransactionCount,
        "expense",
      );
    }
  });

  const transferIncomeParent = `${transferCategoryName} (${labels.income})`;
  const transferExpenseParent = `${transferCategoryName} (${labels.expense})`;
  transferChildNets.forEach((net, childName) => {
    if (net === 0) {
      return;
    }

    const transferChildTotalName =
      childName === transferCategoryName
        ? `${childName} (${labels.total})`
        : childName;
    const absoluteNet = Math.abs(net);
    const transactionCount = transferChildCounts.get(childName) ?? 0;

    if (net > 0) {
      incomeTotal += net;
      markNode(transferIncomeParent, 1, undefined, {
        flowType: "income",
        transactionCount,
      });
      markNode(transferChildTotalName, 0, transferIncomeParent, {
        flowType: "income",
        parentName: transferIncomeParent,
        transactionCount,
      });
      markNode(labels.cashAvailable, 2);
      addLink(
        transferChildTotalName,
        transferIncomeParent,
        net,
        transactionCount,
        "income",
      );
      addLink(
        transferIncomeParent,
        labels.cashAvailable,
        net,
        transactionCount,
        "income",
      );
    } else {
      expenseTotal += absoluteNet;
      markNode(labels.cashAvailable, 2);
      markNode(transferExpenseParent, 3, undefined, {
        flowType: "expense",
        transactionCount,
      });
      markNode(transferChildTotalName, 4, transferExpenseParent, {
        flowType: "expense",
        parentName: transferExpenseParent,
        transactionCount,
      });
      addLink(
        labels.cashAvailable,
        transferExpenseParent,
        absoluteNet,
        transactionCount,
        "expense",
      );
      addLink(
        transferExpenseParent,
        transferChildTotalName,
        absoluteNet,
        transactionCount,
        "expense",
      );
    }
  });

  if (incomeTotal > expenseTotal) {
    markNode(labels.cashAvailable, 2);
    markNode(labels.surplus, 3, undefined, { flowType: "surplus" });
    addLink(
      labels.cashAvailable,
      labels.surplus,
      incomeTotal - expenseTotal,
      0,
      "surplus",
    );
  } else if (expenseTotal > incomeTotal) {
    markNode(labels.deficit, 1, undefined, { flowType: "deficit" });
    markNode(labels.cashAvailable, 2);
    addLink(
      labels.deficit,
      labels.cashAvailable,
      expenseTotal - incomeTotal,
      0,
      "deficit",
    );
  }

  const incomingFlowTotals = new Map<string, number>();
  const outgoingFlowTotals = new Map<string, number>();
  links.forEach(({ source, target, value }) => {
    outgoingFlowTotals.set(
      source,
      (outgoingFlowTotals.get(source) ?? 0) + value,
    );
    incomingFlowTotals.set(
      target,
      (incomingFlowTotals.get(target) ?? 0) + value,
    );
  });

  const nodeFlowTotals = new Map<string, number>();
  nodeNames.forEach((name) => {
    nodeFlowTotals.set(
      name,
      Math.max(
        incomingFlowTotals.get(name) ?? 0,
        outgoingFlowTotals.get(name) ?? 0,
      ),
    );
  });

  const compareByFlow = (left: string, right: string) => {
    const flowDifference =
      (nodeFlowTotals.get(right) ?? 0) - (nodeFlowTotals.get(left) ?? 0);
    return flowDifference !== 0 ? flowDifference : left.localeCompare(right);
  };

  nodeNames.sort((left, right) => {
    const layerDifference =
      (nodeLayers.get(left) ?? 0) - (nodeLayers.get(right) ?? 0);
    if (layerDifference !== 0) {
      return layerDifference;
    }

    const groupDifference = compareByFlow(
      nodeGroups.get(left) ?? left,
      nodeGroups.get(right) ?? right,
    );
    if (groupDifference !== 0) {
      return groupDifference;
    }

    return compareByFlow(left, right);
  });

  nodeNames.forEach((name, index) => nodeIndexes.set(name, index));

  return {
    nodes: nodeNames.map((name) => ({
      name,
      ...nodeMetadata.get(name),
    })),
    links: Array.from(links.values()).map((link) => ({
      source: nodeIndexes.get(link.source)!,
      target: nodeIndexes.get(link.target)!,
      value: link.value,
      transactionCount: link.transactionCount,
      flowType: link.flowType,
    })),
    totalIncome: incomeTotal,
    totalSpending: expenseTotal,
  };
};

interface MonthlySpendingData {
  month: string;
  total: number;
}

export const buildMonthlySpendingChartData = (
  months: Date[],
  transactions: ITransaction[],
  invertData: boolean,
): MonthlySpendingData[] => {
  const monthlySpendingChartData: MonthlySpendingData[] = [];
  months.forEach((month) => {
    const transactionsForMonth = getTransactionsForMonth(transactions, month);

    monthlySpendingChartData.push({
      month: month.toLocaleString("default", {
        month: "numeric",
        year: "2-digit",
      }),
      total:
        transactionsForMonth.reduce(
          (acc, transaction) => acc + transaction.amount,
          0,
        ) * (invertData ? -1 : 1),
    });
  });
  return monthlySpendingChartData;
};
