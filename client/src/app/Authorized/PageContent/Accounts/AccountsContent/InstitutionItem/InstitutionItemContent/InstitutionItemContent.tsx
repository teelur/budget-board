import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { IInstitution } from "~/models/institution";

interface IInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  toggle: () => void;
}

const InstitutionItemContent = (
  props: IInstitutionItemContentProps,
): React.ReactNode => {
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryHeading size="lg">{props.institution.name}</PrimaryHeading>
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
      <StatusText amount={props.totalBalance} size="lg">
        <SensitiveAmount amount={props.totalBalance} />
      </StatusText>
    </Group>
  );
};

export default InstitutionItemContent;
