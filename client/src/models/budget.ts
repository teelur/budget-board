export interface IBudgetCreateRequest {
  month: string;
  category: string;
  limit: number;
  rolloverStartMonth?: string | null;
}

export interface IBudgetUpdateRequest {
  id: string;
  limit: number;
  rolloverStartMonth: string | null;
}

export interface IBudget {
  id: string;
  month: string;
  category: string;
  limit: number;
  /** Month the rollover balance starts accumulating from. Null disables rollover. */
  rolloverStartMonth: string | null;
  /** Balance accumulated since the start month. Negative when earlier months were overspent. */
  rollover: number;
  userId: string;
}

export enum CashFlowValue {
  Positive,
  Neutral,
  Negative,
}
