export const CategoryTypes = {
  Expense: "expense",
  Income: "income",
} as const;

export type CategoryType = (typeof CategoryTypes)[keyof typeof CategoryTypes];

export interface ICategory {
  value: string;
  parent: string;
  categoryType: string;
}

export interface ICategoryCreateRequest extends ICategory {}

export interface ICategoryUpdateRequest extends ICategory {
  id: string;
}

export interface ICategoryResponse extends ICategory {
  id: string;
}

export interface ICategoryNode extends ICategory {
  subCategories: ICategoryNode[];
}

export class CategoryNode implements ICategoryNode {
  subCategories: ICategoryNode[];
  value: string;
  parent: string;
  categoryType: string;

  constructor(category?: ICategory) {
    this.value = category?.value ?? "";
    this.parent = category?.parent ?? "";
    this.categoryType = category?.categoryType ?? CategoryTypes.Expense;
    this.subCategories = [];
  }
}
