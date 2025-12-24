export interface IUserSettings {
  currency: string;
  language: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInTransactionCategories: boolean;
}

export interface IUserSettingsUpdateRequest {
  currency?: string;
  language?: string;
  budgetWarningThreshold?: number;
  forceSyncLookbackMonths?: number;
  disableBuiltInTransactionCategories?: boolean;
}

export class LanguageItem {
  value: string = "";
  label: string = "";
}

export const Languages: LanguageItem[] = [
  { value: "default", label: "system_default" },
  { value: "en-us", label: "en_us" },
];
