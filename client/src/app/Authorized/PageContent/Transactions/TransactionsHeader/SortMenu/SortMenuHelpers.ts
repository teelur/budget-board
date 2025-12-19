export enum Sorts {
  Date,
  Merchant,
  Category,
  Amount,
}

export interface SortOption {
  value: Sorts;
  label: string;
}
