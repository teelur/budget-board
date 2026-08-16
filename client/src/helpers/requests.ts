import { AxiosError } from "axios";

export const accountsQueryKey: string = "accounts";
export const accountTypesQueryKey: string = "accountTypes";
export const applicationUserQueryKey: string = "applicationUser";
export const assetsQueryKey: string = "assets";
export const assetTypesQueryKey: string = "assetTypes";
export const automaticRulesQueryKey: string = "automaticRules";
export const balancesQueryKey: string = "balances";
export const budgetsQueryKey: string = "budgets";
export const goalsQueryKey: string = "goals";
export const institutionsQueryKey: string = "institutions";
export const lunchFlowAccountQueryKey: string = "lunchFlowAccounts";
export const simpleFinOrganizationQueryKey: string = "simpleFinOrganizations";
export const simpleFinAccountQueryKey: string = "simpleFinAccounts";
export const transactionCategoriesQueryKey: string = "transactionCategories";
export const tagSuggestionsQueryKey: string = "tagSuggestions";
export const transactionsQueryKey: string = "transactions";
export const transactionImportJobQueryKey: string = "transactionImportJob";
export const twoFactorAuthQueryKey: string = "twoFactorAuth";
export const userSettingsQueryKey: string = "userSettings";
export const valuesQueryKey: string = "values";
export const widgetSettingsQueryKey: string = "widgetSettings";

export interface ValidationError {
  title: string;
  type: string;
  status: number;
  errors: object;
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === "object" && value !== null;

const collectMessages = (value: unknown, messages: string[]): void => {
  if (typeof value === "string" && value.trim().length > 0) {
    messages.push(value.trim());
    return;
  }

  if (Array.isArray(value)) {
    value.forEach((item) => collectMessages(item, messages));
    return;
  }

  if (isRecord(value)) {
    Object.values(value).forEach((item) => collectMessages(item, messages));
  }
};

const translateStructuredError = (data: unknown): string | undefined => {
  if (!isRecord(data)) {
    return undefined;
  }

  const messages: string[] = [];
  [data.title, data.detail, data.message].forEach((value) =>
    collectMessages(value, messages),
  );
  collectMessages(data.errors, messages);

  const uniqueMessages = [...new Set(messages)];
  return uniqueMessages.length > 0 ? uniqueMessages.join(" ") : undefined;
};

/**
 * Translates an Axios error object into a human-readable error message.
 *
 * @param error - The AxiosError object to translate.
 * @returns A string describing the error, based on the error's response, request, or setup.
 *
 * - If the server responded with an error message (as a string), that message is returned.
 * - If the request was made but no response was received, a generic message is returned.
 * - If the error occurred during request setup, a setup error message is returned.
 */
export const translateAxiosError = (error: AxiosError): string => {
  if (error.response?.data) {
    if (typeof error.response.data === "string") {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      return error.response.data;
    }

    return (
      translateStructuredError(error.response.data) ??
      "An error occurred with an unexpected response format."
    );
  } else if (error.request) {
    // The request was made but no response was received
    return "No response received from the server.";
  }
  // Something happened in setting up the request that triggered an Error
  return "An error occurred while setting up the request.";
};
