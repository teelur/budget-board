import React from "react";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { maskedAmountText } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SensitiveAmountProps {
  amount: number;
  includeCents?: boolean;
  signDisplay?: SignDisplay;
}

const SensitiveAmount = ({
  amount,
  includeCents = true,
  signDisplay = SignDisplay.Auto,
}: SensitiveAmountProps): React.ReactNode => {
  const { intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency } = useUserSettings();

  if (isPrivacyModeEnabled) {
    return maskedAmountText;
  }

  return convertNumberToCurrency(
    amount,
    includeCents,
    preferredCurrency,
    signDisplay,
    intlLocale,
  );
};

export default SensitiveAmount;
