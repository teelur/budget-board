import {
  IBudgetTemplateCategoryItem,
  IBudgetTemplateExport,
  IBudgetValidationResult,
} from "~/models/budgetTemplate";

export const validateBudgetTemplateJson = (
  jsonText: string,
): IBudgetValidationResult => {
  const errors: string[] = [];
  const warnings: string[] = [];

  let parsed: unknown;
  try {
    parsed = JSON.parse(jsonText);
  } catch (err) {
    return {
      isValid: false,
      errors: [
        err instanceof Error
          ? `Invalid JSON syntax: ${err.message}`
          : "Invalid JSON syntax.",
      ],
      warnings: [],
    };
  }

  if (typeof parsed !== "object" || parsed === null || Array.isArray(parsed)) {
    return {
      isValid: false,
      errors: ["JSON root must be an object containing a 'categories' list."],
      warnings: [],
    };
  }

  const root = parsed as Record<string, unknown>;

  if (!Array.isArray(root.categories)) {
    return {
      isValid: false,
      errors: ["Missing or invalid 'categories' array in budget template."],
      warnings: [],
    };
  }

  if (root.categories.length === 0) {
    warnings.push("The imported template contains no categories.");
  }

  const seenCategoryNames = new Set<string>();

  const validateCategoryItem = (
    item: unknown,
    path: string,
    isSubcategory: boolean,
  ): IBudgetTemplateCategoryItem | null => {
    if (typeof item !== "object" || item === null || Array.isArray(item)) {
      errors.push(`${path}: Category entry must be an object.`);
      return null;
    }

    const cat = item as Record<string, unknown>;
    const name = typeof cat.name === "string" ? cat.name.trim() : "";

    if (!name) {
      errors.push(`${path}: 'name' is required and cannot be empty.`);
    } else {
      const lowerName = name.toLowerCase();
      if (seenCategoryNames.has(lowerName)) {
        errors.push(`${path}: Duplicate category name '${name}' found in template.`);
      } else {
        seenCategoryNames.add(lowerName);
      }
    }

    let limit = 0;
    if (typeof cat.limit === "number") {
      if (isNaN(cat.limit) || cat.limit < 0) {
        errors.push(`${path}: 'limit' must be a non-negative number.`);
      } else {
        limit = cat.limit;
      }
    } else if (typeof cat.limit === "string" && !isNaN(Number(cat.limit))) {
      limit = Math.max(0, Number(cat.limit));
    } else if (cat.limit !== undefined) {
      errors.push(`${path}: 'limit' must be a valid number.`);
    }

    const rawType = typeof cat.categoryType === "string" ? cat.categoryType.toLowerCase() : "expense";
    const categoryType = rawType === "income" ? "income" : "expense";
    const rolloverEnabled = Boolean(cat.rolloverEnabled);
    const icon = typeof cat.icon === "string" && cat.icon.trim() ? cat.icon.trim() : null;

    const subcategories: IBudgetTemplateCategoryItem[] = [];
    if (Array.isArray(cat.subcategories)) {
      if (isSubcategory && cat.subcategories.length > 0) {
        errors.push(`${path}: Nested subcategories beyond 1 level are not supported.`);
      } else {
        cat.subcategories.forEach((sub, idx) => {
          const validSub = validateCategoryItem(
            sub,
            `${path}.subcategories[${idx}]`,
            true,
          );
          if (validSub) {
            subcategories.push(validSub);
          }
        });
      }
    }

    return {
      name,
      categoryType,
      limit,
      rolloverEnabled,
      icon,
      subcategories,
    };
  };

  const validCategories: IBudgetTemplateCategoryItem[] = [];
  root.categories.forEach((catItem, idx) => {
    const valid = validateCategoryItem(catItem, `categories[${idx}]`, false);
    if (valid) {
      validCategories.push(valid);
    }
  });

  const template: IBudgetTemplateExport = {
    version: typeof root.version === "number" ? root.version : 1,
    name: typeof root.name === "string" && root.name.trim() ? root.name.trim() : "Imported Budget",
    exportedAt:
      typeof root.exportedAt === "string" && root.exportedAt.trim()
        ? root.exportedAt
        : new Date().toISOString(),
    categories: validCategories,
  };

  return {
    isValid: errors.length === 0,
    errors,
    warnings,
    template: errors.length === 0 ? template : undefined,
  };
};
