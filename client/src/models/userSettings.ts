export interface IUserSettings {
  currency: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInTransactionCategories: boolean;
}

export interface IUserSettingsUpdateRequest {
  currency?: string;
  budgetWarningThreshold?: number;
  forceSyncLookbackMonths?: number;
  disableBuiltInTransactionCategories?: boolean;
}

export enum Currency {
  USD = "USD",
  EUR = "EUR",
  GBP = "GBP",
  JPY = "JPY",
  AUD = "AUD",
  CAD = "CAD",
  CHF = "CHF",
  CNY = "CNY",
  SEK = "SEK",
  NZD = "NZD",
}
