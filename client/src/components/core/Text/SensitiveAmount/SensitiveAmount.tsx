import React from "react";
import { SignDisplay } from "~/helpers/currency";
import { formatSensitiveAmount } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SensitiveAmountProps {
  amount: number;
  includeCents?: boolean;
  currency?: string;
  signDisplay?: SignDisplay;
}

export const useSensitiveAmountFormatter = (): ((
  amount: number,
  includeCents?: boolean,
  signDisplay?: SignDisplay,
  currency?: string,
) => string) => {
  const { intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency } = useUserSettings();

  return React.useCallback(
    (
      amount: number,
      includeCents = true,
      signDisplay = SignDisplay.Auto,
      currency?: string,
    ): string =>
      formatSensitiveAmount(
        amount,
        includeCents,
        currency ?? preferredCurrency,
        signDisplay,
        intlLocale,
        isPrivacyModeEnabled,
      ),
    [intlLocale, isPrivacyModeEnabled, preferredCurrency],
  );
};

const SensitiveAmount = ({
  amount,
  includeCents = true,
  currency,
  signDisplay = SignDisplay.Auto,
}: SensitiveAmountProps): React.ReactNode => {
  const formatAmount = useSensitiveAmountFormatter();

  return formatAmount(amount, includeCents, signDisplay, currency);
};

export default SensitiveAmount;
