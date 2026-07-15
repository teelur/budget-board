import { Group } from "@mantine/core";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

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
        {convertNumberToCurrency(
          props.totalBalance,
          true,
          preferredCurrency,
          SignDisplay.Auto,
          intlLocale,
        )}
      </StatusText>
    </Group>
  );
};

export default NetWorthItem;
