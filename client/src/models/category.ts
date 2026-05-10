export const CategoryTypes = {
  Expense: "expense",
  Income: "income",
} as const;

export type CategoryType = (typeof CategoryTypes)[keyof typeof CategoryTypes];

export interface ICategory {
  value: string;
  parent: string;
}

export interface ITransactionCategory extends ICategory {
  categoryType: string;
}

export interface ICategoryCreateRequest extends ITransactionCategory {}

export interface ICategoryUpdateRequest extends ITransactionCategory {
  id: string;
}

export interface ICategoryResponse extends ITransactionCategory {
  id: string;
}

export interface ICategoryNode extends ITransactionCategory {
  subCategories: ICategoryNode[];
}

export class CategoryNode implements ICategoryNode {
  subCategories: ICategoryNode[];
  value: string;
  parent: string;
  categoryType: string;

  constructor(category?: ICategory & { categoryType?: string }) {
    this.value = category?.value ?? "";
    this.parent = category?.parent ?? "";
    this.categoryType = category?.categoryType ?? CategoryTypes.Expense;
    this.subCategories = [];
  }
}
