import type { TFunction } from "i18next";
import {
  IBudgetTemplateCategoryItem,
  IBudgetTemplateExport,
  IBudgetValidationResult,
} from "~/models/budgetTemplate";

export const validateBudgetTemplateJson = (
  jsonText: string,
  t: TFunction,
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
          ? t("budget_import_validation_invalid_json", { message: err.message })
          : t("budget_import_validation_invalid_json_generic"),
      ],
      warnings: [],
    };
  }

  if (typeof parsed !== "object" || parsed === null || Array.isArray(parsed)) {
    return {
      isValid: false,
      errors: [t("budget_import_validation_root_object")],
      warnings: [],
    };
  }

  const root = parsed as Record<string, unknown>;

  if (!Array.isArray(root.categories)) {
    return {
      isValid: false,
      errors: [t("budget_import_validation_categories_array")],
      warnings: [],
    };
  }

  if (root.categories.length === 0) {
    warnings.push(t("budget_import_validation_empty_categories"));
  }

  const seenCategoryNames = new Set<string>();

  const validateCategoryItem = (
    item: unknown,
    path: string,
    isSubcategory: boolean,
  ): IBudgetTemplateCategoryItem | null => {
    if (typeof item !== "object" || item === null || Array.isArray(item)) {
      errors.push(t("budget_import_validation_category_object", { path }));
      return null;
    }

    const cat = item as Record<string, unknown>;
    const name = typeof cat.name === "string" ? cat.name.trim() : "";

    if (!name) {
      errors.push(t("budget_import_validation_name_required", { path }));
    } else {
      const lowerName = name.toLowerCase();
      if (seenCategoryNames.has(lowerName)) {
        errors.push(
          t("budget_import_validation_duplicate_category", { path, name }),
        );
      } else {
        seenCategoryNames.add(lowerName);
      }
    }

    let limit = 0;
    const limitWasOmitted = cat.limit === undefined;
    if (typeof cat.limit === "number") {
      if (isNaN(cat.limit) || cat.limit < 0) {
        errors.push(t("budget_import_validation_limit_nonnegative", { path }));
      } else {
        limit = cat.limit;
      }
    } else if (typeof cat.limit === "string") {
      const parsedLimit = Number(cat.limit);
      if (!Number.isFinite(parsedLimit)) {
        errors.push(t("budget_import_validation_limit_number", { path }));
      } else if (parsedLimit < 0) {
        errors.push(t("budget_import_validation_limit_nonnegative", { path }));
      } else {
        limit = parsedLimit;
      }
    } else if (cat.limit !== undefined) {
      errors.push(t("budget_import_validation_limit_number", { path }));
    }

    const rawType = typeof cat.categoryType === "string" ? cat.categoryType.toLowerCase() : "expense";
    const categoryType = rawType === "income" ? "income" : "expense";
    let rolloverEnabled = false;
    if (cat.rolloverEnabled !== undefined) {
      if (typeof cat.rolloverEnabled !== "boolean") {
        errors.push(t("budget_import_validation_rollover_boolean", { path }));
      } else {
        rolloverEnabled = cat.rolloverEnabled;
      }
    }
    const icon = typeof cat.icon === "string" && cat.icon.trim() ? cat.icon.trim() : null;

    const subcategories: IBudgetTemplateCategoryItem[] = [];
    if (Array.isArray(cat.subcategories)) {
      if (isSubcategory && cat.subcategories.length > 0) {
        errors.push(t("budget_import_validation_nested_subcategories", { path }));
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
    if (limitWasOmitted) {
      limit = subcategories.reduce((total, subcategory) => total + subcategory.limit, 0);
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
    name:
      typeof root.name === "string" && root.name.trim()
        ? root.name.trim()
        : t("budget_import_default_template_name"),
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
