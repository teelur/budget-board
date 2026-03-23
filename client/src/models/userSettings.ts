export interface IUserSettings {
  currency: string;
  language: string;
  dateFormat: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInTransactionCategories: boolean;
  toshlMetadataSyncDirection: string;
  toshlSyncLookbackMonths: number;
  toshlAutoSyncIntervalHours: number;
  toshlFullSyncStatus: string;
  toshlFullSyncQueuedAt?: Date;
  toshlFullSyncStartedAt?: Date;
  toshlFullSyncCompletedAt?: Date;
  toshlFullSyncError: string;
  toshlFullSyncProgressPercent: number;
  toshlFullSyncProgressDescription: string;
  enableAutoCategorizer: boolean;
  autoCategorizerModelOID?: number;
  autoCategorizerLastTrained?: Date;
  autoCategorizerModelStartDate?: Date;
  autoCategorizerModelEndDate?: Date;
  autoCategorizerMinimumProbabilityPercentage: number;
}

export const ToshlMetadataSyncDirections = {
  BudgetBoard: "budgetboard",
  Toshl: "toshl",
} as const;

export const ToshlFullSyncStatuses = {
  Idle: "idle",
  Queued: "queued",
  Running: "running",
  Succeeded: "succeeded",
  Failed: "failed",
} as const;

export interface IUserSettingsUpdateRequest {
  currency?: string;
  language?: string;
  dateFormat?: string;
  budgetWarningThreshold?: number;
  forceSyncLookbackMonths?: number;
  disableBuiltInTransactionCategories?: boolean;
  toshlMetadataSyncDirection?: string;
  toshlSyncLookbackMonths?: number;
  toshlAutoSyncIntervalHours?: number;
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
  { value: "en-us", label: "english_us" },
  { value: "de", label: "german" },
  { value: "fr", label: "french" },
  { value: "zh-hans", label: "chinese_simplified" },
];

export class DateFormatItem {
  value: string = "";
  label: string = "";
}

export const DATE_SEPARATOR_PLACEHOLDER = "{sep}";

export const DateFormats: DateFormatItem[] = [
  { value: "default", label: "system_default" },
  {
    value: `MM${DATE_SEPARATOR_PLACEHOLDER}DD${DATE_SEPARATOR_PLACEHOLDER}YYYY`,
    label: "mm_dd_yyyy",
  },
  {
    value: `DD${DATE_SEPARATOR_PLACEHOLDER}MM${DATE_SEPARATOR_PLACEHOLDER}YYYY`,
    label: "dd_mm_yyyy",
  },
  {
    value: `YYYY${DATE_SEPARATOR_PLACEHOLDER}MM${DATE_SEPARATOR_PLACEHOLDER}DD`,
    label: "yyyy_mm_dd",
  },
];
