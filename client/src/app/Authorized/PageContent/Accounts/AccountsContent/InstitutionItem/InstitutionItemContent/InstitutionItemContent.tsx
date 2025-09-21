import { ActionIcon, Group, Text } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
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
      <Group>
        <Text fw={600} size="md">
          {props.institution.name}
        </Text>
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
      <Text fw={600} size="md" c={props.totalBalance < 0 ? "red" : "green"}>
        {convertNumberToCurrency(props.totalBalance, true, props.userCurrency)}
      </Text>
    </Group>
  );
};

export default InstitutionItemContent;
