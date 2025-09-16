import { CategoryNode, ICategory } from "~/models/category";
import { areStringsEqual } from "./utils";
import { uncategorizedTransactionCategory } from "~/models/transaction";

/**
 * Builds an alphabetized tree of category nodes and their children
 * @param categories The array of category data
 * @returns An array of CategoryNode which each contain an array of its children.
 */
export const buildCategoriesTree = (
  categories: ICategory[]
): CategoryNode[] => {
  const roots: CategoryNode[] = [];

  // Push all the parent nodes to the roots array
  categories
    .filter((c) => c.parent.length === 0)
    .forEach((parent) => {
      roots.push(new CategoryNode(parent));
    });

  // Populate the children branches for each parent root node
  categories
    .filter((c) => c.parent.length !== 0)
    .forEach((category) => {
      const parent: CategoryNode | undefined = roots.find((c) =>
        areStringsEqual(c.value, category.parent)
      );

      if (parent) {
        parent.subCategories.push(new CategoryNode(category));
      }
    });

  roots.sort((a, b) =>
    a.value.toLocaleLowerCase().localeCompare(b.value.toLocaleLowerCase())
  );

  return roots;
};

/**
 * Searches the given categories for a category matching the provided string.
 * If a matching category is found, returns its value; otherwise returns "Uncategorized".
 *
 * The comparison is case-insensitive, leveraging the utility function areStringsEqual.
 *
 * @param {string} categoryString - The category to look up in the categories array.
 * @param {ICategory[]} categories - The array of category objects.
 * @returns {string} The matching category's value, or "Uncategorized" if no match is found.
 */
export const getFormattedCategoryValue = (
  categoryString: string,
  categories: ICategory[]
): string => {
  const foundCategory = categories.find((c) =>
    areStringsEqual(c.value, categoryString)
  );

  return foundCategory?.value ?? uncategorizedTransactionCategory;
};

/**
 * Checks if the specified category is a root (parent) category.
 *
 * This function locates a matching category by the given value, then checks
 * if its parent field is empty (length === 0). If so, the category
 * is considered a parent category.
 *
 * @param {string} categoryValue - The category value to look for.
 * @param {ICategory[]} categories - An array of all available categories.
 * @returns {boolean} True if the category is a parent category, false otherwise.
 */
export const getIsParentCategory = (
  categoryValue: string,
  categories: ICategory[]
): boolean => {
  // Handle the special case for "Uncategorized"
  if (areStringsEqual(categoryValue, uncategorizedTransactionCategory)) {
    return true;
  }

  return (
    categories.find((c) => areStringsEqual(c.value, categoryValue))?.parent
      .length === 0
  );
};

/**
 * Retrieves the category's parent value, or returns the category's own value if it has no parent.
 * If the category cannot be found, an empty string is returned.
 *
 * @param categoryValue - The category value to locate.
 * @param categories - The list of available categories.
 * @returns The parent category's value, the category's own value if it is a root, or an empty string if not found.
 */
export const getParentCategory = (
  categoryValue: string,
  categories: ICategory[]
): string => {
  // Handle the special case for "Uncategorized"
  if (areStringsEqual(categoryValue, uncategorizedTransactionCategory)) {
    return uncategorizedTransactionCategory;
  }

  const category = categories.find((c) =>
    areStringsEqual(c.value, categoryValue)
  );

  if (category == null) {
    return "";
  }

  return category.parent === "" ? category.value : category.parent;
};

/**
 * Retrieves the subcategories of a specified category by filtering
 * the provided list of categories whose parent matches the given value,
 * ignoring case differences.
 *
 * @param {string} categoryValue - The parent category value.
 * @param {ICategory[]} categories - The list of categories to search.
 * @returns {ICategory[]} An array of matching sub-category objects.
 */
export const getSubCategories = (
  categoryValue: string,
  categories: ICategory[]
): ICategory[] =>
  categories.filter((c) => areStringsEqual(c.parent, categoryValue));
