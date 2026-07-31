import { Group } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  return (
    <Group
      p={0}
      justify="space-between"
      align="center"
      wrap="nowrap"
      gap="0.25rem"
    >
      <PrimaryText fw={600}>{props.title}</PrimaryText>
      <StatusText amount={props.totalBalance}>
        <SensitiveAmount amount={props.totalBalance} />
      </StatusText>
    </Group>
  );
};

export default NetWorthItem;
