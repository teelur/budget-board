export const RecurringCadenceUnits = {
  Day: "Day",
  Week: "Week",
  Month: "Month",
  Year: "Year",
} as const;

export type RecurringCadenceUnit =
  (typeof RecurringCadenceUnits)[keyof typeof RecurringCadenceUnits];

export const RecurringCadenceModes = {
  Interval: "Interval",
  PerUnit: "PerUnit",
} as const;

export type RecurringCadenceMode =
  (typeof RecurringCadenceModes)[keyof typeof RecurringCadenceModes];

export interface IRecurringCadence {
  version: number;
  unit: RecurringCadenceUnit;
  interval: number;
  mode?: RecurringCadenceMode;
  unsupported?: boolean;
}

export type RecurringCadence = IRecurringCadence;

export const defaultRecurringCadence: IRecurringCadence = {
  version: 1,
  unit: RecurringCadenceUnits.Month,
  interval: 1,
};

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
  cadence: IRecurringCadence;
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
