export interface IToshlCategoryMappingItem {
  toshlId: string;
  toshlName: string;
  toshlType: string;
  toshlParentName: string;
  budgetBoardCategory: string;
  suggestedBudgetBoardCategory: string;
}

export interface IToshlCategoryMappingsResponse {
  items: IToshlCategoryMappingItem[];
}

export interface IToshlCategoryMappingUpdateItem {
  toshlId: string;
  toshlName: string;
  toshlType: string;
  budgetBoardCategory: string;
}

export interface IToshlCategoryMappingsUpdateRequest {
  items: IToshlCategoryMappingUpdateItem[];
}
