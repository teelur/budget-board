export interface IBudgetTemplateCategoryItem {
  name: string;
  categoryType: "expense" | "income" | string;
  limit: number;
  rolloverEnabled: boolean;
  icon?: string | null;
  subcategories: IBudgetTemplateCategoryItem[];
}

export interface IBudgetTemplateExport {
  version: number;
  name: string;
  exportedAt: string;
  categories: IBudgetTemplateCategoryItem[];
}

export interface IBudgetImportCategoryCreateRequest {
  name: string;
  parent?: string | null;
  categoryType: string;
  icon?: string | null;
}

export interface IBudgetImportItemRequest {
  category: string;
  limit: number;
  rolloverEnabled: boolean;
}

export interface IBudgetImportRequest {
  month: string;
  items: IBudgetImportItemRequest[];
  newCategories: IBudgetImportCategoryCreateRequest[];
  overwriteExisting: boolean;
}

export interface IBudgetValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
  template?: IBudgetTemplateExport;
}

export type CategoryResolutionAction = "create" | "map" | "skip";

export interface IMissingCategoryResolution {
  categoryName: string;
  parentName: string | null;
  categoryType: string;
  icon: string | null;
  action: CategoryResolutionAction;
  mappedCategory: string;
}
