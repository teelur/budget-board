import React from "react";
import { SignDisplay } from "~/helpers/currency";
import { formatSensitiveAmount } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

export const useSensitiveAmountFormatter = (): ((
  amount: number,
  signDisplay?: SignDisplay,
  currency?: string,
  decimalPlaces?: number,
) => string) => {
  const { intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency, decimalPlaces } = useUserSettings();

  return React.useCallback(
    (
      amount: number,
      signDisplay = SignDisplay.Auto,
      currency?: string,
      decimalPlacesOverride?: number,
    ): string =>
      formatSensitiveAmount(
        amount,
        decimalPlacesOverride ?? decimalPlaces,
        currency ?? preferredCurrency,
        signDisplay,
        intlLocale,
        isPrivacyModeEnabled,
      ),
    [decimalPlaces, intlLocale, isPrivacyModeEnabled, preferredCurrency],
  );
};