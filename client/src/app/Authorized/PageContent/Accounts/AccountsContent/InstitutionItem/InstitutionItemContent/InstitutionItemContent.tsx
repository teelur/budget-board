import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { IInstitution } from "~/models/institution";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface IInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  userCurrency: string;
  toggle: () => void;
}

const InstitutionItemContent = (
  props: IInstitutionItemContentProps,
): React.ReactNode => {
  const { intlLocale } = useLocale();
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">{props.institution.name}</PrimaryText>
        <ActionIcon
          variant="transparent"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.toggle();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <StatusText amount={props.totalBalance} size="md">
        {convertNumberToCurrency(
          props.totalBalance,
          true,
          props.userCurrency,
          SignDisplay.Auto,
          intlLocale,
        )}
      </StatusText>
    </Group>
  );
};

export default InstitutionItemContent;
