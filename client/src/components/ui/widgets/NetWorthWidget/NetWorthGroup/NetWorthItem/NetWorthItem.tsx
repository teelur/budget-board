import { Group } from "@mantine/core";
import { AmountText } from "@teelur/budget-board-ui";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();

  return (
    <Group
      p={0}
      justify="space-between"
      align="center"
      wrap="nowrap"
      gap="0.25rem"
    >
      <PrimaryText>{props.title}</PrimaryText>
      <AmountText
        amount={props.totalBalance}
        size="md"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={decimalPlaces}
      />
    </Group>
  );
};

export default NetWorthItem;
