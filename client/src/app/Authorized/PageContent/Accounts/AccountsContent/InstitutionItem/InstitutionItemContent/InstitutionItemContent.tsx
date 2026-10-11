import { Group } from "@mantine/core";
import { ActionIcon, AmountText, HeadingText } from "@teelur/budget-board-ui";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { PencilIcon } from "lucide-react";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { IInstitution } from "~/models/institution";

interface IInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  toggle: () => void;
}

const InstitutionItemContent = (
  props: IInstitutionItemContentProps,
): React.ReactNode => {
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <HeadingText tone="heading" level={4}>
          {props.institution.name}
        </HeadingText>
        <ActionIcon
          variant="ghost"
          color="primary"
          size="xs"
          onClick={(e) => {
            e.stopPropagation();
            props.toggle();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <AmountText
        amount={props.totalBalance}
        size="lg"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={decimalPlaces}
      />
    </Group>
  );
};

export default InstitutionItemContent;
