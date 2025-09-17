import { CashFlowValue, IBudget } from "~/models/budget";
import { CategoryNode, ICategory, ICategoryNode } from "~/models/category";
import { areStringsEqual } from "./utils";
import {
  getFormattedCategoryValue,
  getIsParentCategory,
  getParentCategory,
  getSubCategories,
} from "./category";

export enum BudgetGroup {
  Income,
  Spending,
}

/**
 * Determines the cash flow value for a given date based on a monthly totals map.
 *
 * The function retrieves the monthly total (if any) for the specified date from
 * the provided map, then decides whether the result is positive, negative, or
 * neutral, returning the corresponding CashFlowValue enum.
 *
 * @param {Map<number, number>} timeToMonthlyTotalsMap - Map of timestamp to monthly totals.
 * @param {Date} date - The date used to look up the monthly total.
 * @returns {CashFlowValue} Indicates if the cash flow is Positive, Negative, or Neutral.
 */
export const getCashFlowValue = (
  timeToMonthlyTotalsMap: Map<number, number>,
  date: Date
): CashFlowValue => {
  const cashFlow = timeToMonthlyTotalsMap.get(date.getTime()) ?? 0;
  if (cashFlow > 0) {
    return CashFlowValue.Positive;
  } else if (cashFlow < 0) {
    return CashFlowValue.Negative;
  }
  return CashFlowValue.Neutral;
};

/**
 * Determines the budget group for a given category.
 *
 * If the category name is "Income", returns BudgetGroup.Income.
 * Otherwise, returns BudgetGroup.Spending.
 *
 * @param {string} category - The category's name.
 * @returns {BudgetGroup} - The budget group (Income or Spending).
 */
export const getBudgetGroupForCategory = (category: string): BudgetGroup => {
  if (areStringsEqual(category, "Income")) {
    return BudgetGroup.Income;
  }
  return BudgetGroup.Spending;
};

/**
 * Determines the sign for a given category based on its budget group.
 *
 * If the category is in the Spending group, it returns -1. Otherwise, it returns 1.
 *
 * @param {string} category - The category's name.
 * @param {ICategory[]} transactionCategories - List of all categories for lookups.
 * @returns {number} - The sign for the category.
 */
export const getSignForBudget = (
  category: string,
  transactionCategories: ICategory[]
): number => {
  switch (
    getBudgetGroupForCategory(
      getParentCategory(category, transactionCategories)
    )
  ) {
    case BudgetGroup.Spending:
      return -1;
    case BudgetGroup.Income:
    default:
      return 1;
  }
};

/**
 * Filters a list of budgets by the specified budget group (income or spending).
 *
 * @param {IBudget[] | undefined} budgetData - The list of budgets to filter.
 * @param {BudgetGroup} budgetGroup - The target budget group (Income or Spending).
 * @param {ICategory[]} transactionCategories - All available categories for lookups.
 * @returns {IBudget[]} The filtered list of budgets.
 */
export const getBudgetsForGroup = (
  budgetData: IBudget[] | undefined,
  budgetGroup: BudgetGroup,
  transactionCategories: ICategory[]
): IBudget[] => {
  if (budgetData == null) {
    return [];
  }

  if (budgetGroup === BudgetGroup.Income) {
    return (
      budgetData.filter((b) =>
        areStringsEqual(
          getParentCategory(b.category, transactionCategories),
          "income"
        )
      ) ?? []
    );
  } else if (budgetGroup === BudgetGroup.Spending) {
    return (
      budgetData.filter(
        (b) =>
          !areStringsEqual(
            getParentCategory(b.category, transactionCategories),
            "income"
          )
      ) ?? []
    );
  }
  return budgetData;
};

/**
 * Calculates the total limit from the provided budget data.
 *
 * @param {IBudget[]} budgetData - Array of budgets.
 * @returns {number} The sum of all budget limits.
 */
export const sumBudgetAmounts = (budgetData: IBudget[]): number => {
  return budgetData.reduce((n, { limit }) => n + limit, 0);
};

export enum BudgetValueType {
  Expense,
  Income,
  Total,
}

export const getBudgetValueColor = (
  amount: number,
  total: number,
  type: BudgetValueType,
  warningThreshold: number
): string => {
  if (type === BudgetValueType.Income) {
    if (amount < total) {
      return "var(--mantine-primary-color-light-color)";
    }
    return "green";
  }

  if (type === BudgetValueType.Expense) {
    {
      const invertedAmount = amount * -1;
      if (invertedAmount > total) {
        return "red";
      }
      if (invertedAmount >= total * (warningThreshold / 100)) {
        return "yellow";
      }
      return "green";
    }
  }

  if (type === BudgetValueType.Total) {
    if (amount < 0) {
      return "red";
    } else if (amount > 0) {
      return "green";
    }
  }

  return "var(--mantine-color-text)";
};

/**
 * Retrieves the total amount for the specified budget category by summing the category's own total
 * with any child categories' totals (if the category is a parent).
 *
 * @param {string} budgetCategory - The category name to retrieve total for.
 * @param {Map<string, number>} categoryToTransactionsTotalMap - A map from category name to transaction totals.
 * @param {ICategory[]} categories - An array of all categories for lookups.
 * @returns {number} The total amount for the given category (including child categories if applicable).
 */
export const getBudgetAmount = (
  budgetCategory: string,
  categoryToTransactionsTotalMap: Map<string, number>,
  categories: ICategory[]
): number => {
  if (getIsParentCategory(budgetCategory, categories)) {
    const children = getSubCategories(budgetCategory, categories);

    const childrenTotal = children.reduce(
      (acc, category) =>
        acc +
        (categoryToTransactionsTotalMap.get(
          category.value.toLocaleLowerCase()
        ) ?? 0),
      0
    );

    return (
      childrenTotal + (categoryToTransactionsTotalMap.get(budgetCategory) ?? 0)
    );
  }

  return categoryToTransactionsTotalMap.get(budgetCategory) ?? 0;
};

/**
 * Groups a list of budgets by their category (case-insensitive).
 *
 * The function first sorts the budgets alphabetically by category,
 * then reduces them into a Map keyed by the lowercase category name,
 * storing an array of budgets for each category.
 *
 * @param {IBudget[]} budgets - An array of budget objects.
 * @returns {Map<string, IBudget[]>} - A map from category to list of budgets.
 */
export const buildCategoryToBudgetsMap = (
  budgets: IBudget[]
): Map<string, IBudget[]> =>
  budgets
    .sort((a: IBudget, b: IBudget) => {
      if (a.category.toUpperCase() < b.category.toUpperCase()) {
        return -1;
      } else if (a.category.toUpperCase() > b.category.toUpperCase()) {
        return 1;
      }
      return 0;
    })
    .reduce(
      (budgetMap: any, item: IBudget) =>
        budgetMap.set(item.category.toLocaleLowerCase(), [
          ...(budgetMap.get(item.category.toLocaleLowerCase()) || []),
          item,
        ]),
      new Map()
    );

/**
 * Builds a hierarchical tree structure of budget categories from the provided budgets and categories.
 *
 * Parent categories are added as root nodes, and subcategories are added as children under their
 * respective parents. Only categories that are not parent categories themselves are added as subcategories.
 *
 * @param budgets - An array of budget objects to be organized into the category tree.
 * @param categories - An array of all available categories used to determine parent-child relationships.
 * @returns An array of `ICategoryNode` objects representing the root nodes of the category tree, each with their subcategories populated.
 */
export const buildBudgetCategoryTree = (
  budgets: IBudget[],
  categories: ICategory[]
): ICategoryNode[] => {
  const categoryTree: ICategoryNode[] = [];

  budgets.forEach((budget) => {
    const parentCategory = getParentCategory(budget.category, categories);

    if (
      !categoryTree.some((category) =>
        areStringsEqual(category.value, parentCategory)
      )
    ) {
      categoryTree.push(
        new CategoryNode({
          value: getFormattedCategoryValue(parentCategory, categories),
          parent: "",
        })
      );
    }

    if (!getIsParentCategory(budget.category, categories)) {
      const parent = categoryTree.find((category) =>
        areStringsEqual(category.value, parentCategory)
      );

      if (parent) {
        parent.subCategories.push(
          new CategoryNode({
            value: getFormattedCategoryValue(budget.category, categories),
            parent: parentCategory,
          })
        );
      }
    }
  });

  return categoryTree;
};

export const getTotalLimitForCategory = (
  budgets: IBudget[],
  category: ICategoryNode
): number => {
  if (budgets.some((b) => areStringsEqual(b.category, category.value))) {
    return budgets.reduce((total, budget) => {
      if (areStringsEqual(budget.category, category.value)) {
        return total + budget.limit;
      }

      return total;
    }, 0);
  }

  return budgets.reduce((total, budget) => {
    if (
      category.subCategories.some((subCategory) =>
        areStringsEqual(subCategory.value, budget.category)
      )
    ) {
      return total + budget.limit;
    }

    return total;
  }, 0);
};

export const buildCategoryToLimitsMap = (
  budgets: IBudget[],
  categories: ICategoryNode[]
): Map<string, number> => {
  const categoryToLimitsMap = new Map<string, number>();

  budgets.forEach((budget) => {
    if (categoryToLimitsMap.has(budget.category.toLocaleLowerCase())) {
      categoryToLimitsMap.set(
        budget.category.toLocaleLowerCase(),
        categoryToLimitsMap.get(budget.category.toLocaleLowerCase())! +
          budget.limit
      );
    } else {
      categoryToLimitsMap.set(
        budget.category.toLocaleLowerCase(),
        budget.limit
      );
    }

    // If the budget is for a subcategory, add the limit to the parent category
    if (!categories.some((c) => areStringsEqual(c.value, budget.category))) {
      const parentCategory =
        categories
          .flatMap((c) => c.subCategories)
          .find((c) => areStringsEqual(c.value, budget.category))?.parent ?? "";

      // We only want to add the limit to the parent category if it is not already in the budgets
      if (
        !parentCategory ||
        budgets.some((b) => areStringsEqual(b.category, parentCategory))
      ) {
        return;
      }

      if (categoryToLimitsMap.has(parentCategory.toLocaleLowerCase())) {
        categoryToLimitsMap.set(
          parentCategory.toLocaleLowerCase(),
          categoryToLimitsMap.get(parentCategory.toLocaleLowerCase())! +
            budget.limit
        );
      } else {
        categoryToLimitsMap.set(
          parentCategory.toLocaleLowerCase(),
          budget.limit
        );
      }
    }
  });

  return categoryToLimitsMap;
};
