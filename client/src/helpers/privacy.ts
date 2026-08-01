import { convertNumberToCurrency, SignDisplay } from "./currency";

export const privacyModeStorageKey = "budgetBoardPrivacyMode";
export const maskedAmountText = "••••";

export const formatSensitiveAmount = (
  amount: number,
  includeCents: boolean,
  currency: string,
  signDisplay: SignDisplay,
  locale: string,
  isPrivacyModeEnabled: boolean,
): string => {
  if (isPrivacyModeEnabled) {
    return maskedAmountText;
  }

  return convertNumberToCurrency(
    amount,
    includeCents,
    currency,
    signDisplay,
    locale,
  );
};

export const formatSensitiveText = (
  text: string,
  isPrivacyModeEnabled: boolean,
): string => {
  if (isPrivacyModeEnabled) {
    return maskedAmountText;
  }

  return text;
};

export const getStoredPrivacyMode = (): boolean => {
  if (typeof window === "undefined") {
    return false;
  }

  try {
    return window.localStorage.getItem(privacyModeStorageKey) === "true";
  } catch {
    return false;
  }
};

export const storePrivacyMode = (enabled: boolean): void => {
  if (typeof window === "undefined") {
    return;
  }

  try {
    window.localStorage.setItem(privacyModeStorageKey, String(enabled));
  } catch {
    return;
  }
};
