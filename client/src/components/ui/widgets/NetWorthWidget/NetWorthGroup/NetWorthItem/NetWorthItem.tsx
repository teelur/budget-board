import { Group } from "@mantine/core";
import { AmountText } from "@teelur/budget-board-ui";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { isPrivacyModeEnabled } = usePrivacyMode();

  return (
    <Group
      p={0}
      justify="space-between"
      align="center"
      wrap="nowrap"
      gap="0.25rem"
    >
      <PrimaryText fw={600}>{props.title}</PrimaryText>
      <AmountText
        amount={props.totalBalance}
        size="md"
        disableStatusColor={isPrivacyModeEnabled}
      >
        <SensitiveAmount amount={props.totalBalance} />
      </AmountText>
    </Group>
  );
};

export default NetWorthItem;
