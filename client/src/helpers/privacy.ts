export const privacyModeStorageKey = "budgetBoardPrivacyMode";
export const maskedAmountText = "••••";

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
