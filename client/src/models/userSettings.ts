export interface IUserSettings {
  currency: string;
  language: string;
  dateFormat: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInTransactionCategories: boolean;
  enableAutoCategorizer: boolean;
  autoCategorizerModelOID?: number;
  autoCategorizerLastTrained?: Date;
  autoCategorizerModelStartDate?: Date;
  autoCategorizerModelEndDate?: Date;
  autoCategorizerMinimumProbabilityPercentage: number;
}

export interface IUserSettingsUpdateRequest {
  currency?: string;
  language?: string;
  dateFormat?: string;
  budgetWarningThreshold?: number;
  forceSyncLookbackMonths?: number;
  disableBuiltInTransactionCategories?: boolean;
  enableAutoCategorizer?: boolean;
  autoCategorizerModelOID?: number;
  autoCategorizerLastTrained?: Date;
  autoCategorizerModelStartDate?: Date;
  autoCategorizerModelEndDate?: Date;
  autoCategorizerMinimumProbabilityPercentage?: number;
}

export class LanguageItem {
  value: string = "";
  label: string = "";
}

export const Languages: LanguageItem[] = [
  { value: "default", label: "system_default" },
  { value: "en-us", label: "en_us" },
  { value: "de", label: "de" },
  { value: "zh-hans", label: "zh_hans" },
];

export class DateFormatItem {
  value: string = "";
  label: string = "";
}

export const DateFormats: DateFormatItem[] = [
  { value: "default", label: "system_default" },
  { value: "MM/DD/YYYY", label: "mm_dd_yyyy" },
  { value: "DD/MM/YYYY", label: "dd_mm_yyyy" },
  { value: "YYYY/MM/DD", label: "yyyy_mm_dd" },
];
