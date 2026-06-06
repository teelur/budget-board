export interface IBudgetCreateRequest {
  month: string;
  category: string;
  limit: number;
}

export interface IBudgetUpdateRequest {
  id: string;
  limit: number;
}

export interface IBudget {
  id: string;
  month: string;
  category: string;
  limit: number;
  userId: string;
}

export enum CashFlowValue {
  Positive,
  Neutral,
  Negative,
}
