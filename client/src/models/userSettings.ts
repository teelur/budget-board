export interface IUserSettings {
  currency: string;
  language: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInTransactionCategories: boolean;
  enableAutoCategorizer: boolean;
  autoCategorizerModelOID: number;
  autoCategorizerLastTrained: Date;
  autoCategorizerModelStartDate: Date;
  autoCategorizerModelEndDate: Date;
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
}

export class LanguageItem {
  value: string = "";
  label: string = "";
}

export const Languages: LanguageItem[] = [
  { value: "default", label: "system_default" },
  { value: "en-us", label: "en_us" },
];
