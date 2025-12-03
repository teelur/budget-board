import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IInstitution } from "~/models/institution";

interface IInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  userCurrency: string;
  toggle: () => void;
}

const InstitutionItemContent = (
  props: IInstitutionItemContentProps
): React.ReactNode => {
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
        {convertNumberToCurrency(props.totalBalance, true, props.userCurrency)}
      </StatusText>
    </Group>
  );
};

export default InstitutionItemContent;
