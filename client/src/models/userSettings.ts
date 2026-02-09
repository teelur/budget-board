export interface IUserSettings {
  currency: string;
  language: string;
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
