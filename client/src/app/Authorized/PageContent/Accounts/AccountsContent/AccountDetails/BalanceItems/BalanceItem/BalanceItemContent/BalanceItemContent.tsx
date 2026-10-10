import { Group } from "@mantine/core";
import { ActionIcon, AmountText, BodyText } from "@teelur/budget-board-ui";
import { PencilIcon } from "lucide-react";
import { IBalanceResponse } from "~/models/balance";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface BalanceItemContentProps {
  balance: IBalanceResponse;
  doSelect: () => void;
}

const BalanceItemContent = (
  props: BalanceItemContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <BodyText tone="primary" size="sm" fw={500}>
          {dayjs(props.balance.date).format(longDateFormat)}
        </BodyText>
        <ActionIcon
          variant="ghost"
          color="primary"
          size="compact-xs"
          onClick={(e) => {
            e.stopPropagation();
            props.doSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <AmountText
        amount={props.balance.amount}
        size="md"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={decimalPlaces}
      />
    </Group>
  );
};

export default BalanceItemContent;
