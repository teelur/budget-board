import { Group } from "@mantine/core";
import { ActionIcon, AmountText } from "@teelur/budget-board-ui";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { PencilIcon } from "lucide-react";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
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
        <PrimaryHeading size="lg">{props.institution.name}</PrimaryHeading>
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
