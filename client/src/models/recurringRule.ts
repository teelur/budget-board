export const RecurringCadences = {
  Weekly: "Weekly",
  Biweekly: "Biweekly",
  Monthly: "Monthly",
  Yearly: "Yearly",
} as const;

export type RecurringCadence =
  (typeof RecurringCadences)[keyof typeof RecurringCadences];

export const RecurringAmountModes = {
  Fixed: "Fixed",
  Automatic: "Automatic",
} as const;

export type RecurringAmountMode =
  (typeof RecurringAmountModes)[keyof typeof RecurringAmountModes];

export interface IRecurringRuleCreateRequest {
  accountID: string;
  merchantName: string | null;
  category: string | null;
  subcategory: string | null;
  cadence: RecurringCadence;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
  amountMode: RecurringAmountMode;
  amount: number;
}

export interface IRecurringRuleUpdateRequest extends IRecurringRuleCreateRequest {
  id: string;
}

export interface IRecurringRuleResponse extends IRecurringRuleCreateRequest {
  id: string;
  accountName: string;
  matchedTransactionCount: number;
  nextOccurrenceDate: string | null;
}

export interface IRecurringForecastOccurrence {
  ruleID: string;
  date: string;
  amount: number;
  merchantName: string | null;
  accountID: string;
  accountName: string;
  category: string | null;
  subcategory: string | null;
}