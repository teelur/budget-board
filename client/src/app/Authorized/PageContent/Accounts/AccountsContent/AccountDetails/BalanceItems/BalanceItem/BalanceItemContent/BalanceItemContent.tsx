import { Group } from "@mantine/core";
import { ActionIcon, AmountText } from "@teelur/budget-board-ui";
import { PencilIcon } from "lucide-react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { IBalanceResponse } from "~/models/balance";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface BalanceItemContentProps {
  balance: IBalanceResponse;
  userCurrency: string;
  doSelect: () => void;
}

const BalanceItemContent = (
  props: BalanceItemContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">
          {dayjs(props.balance.date).format(longDateFormat)}
        </PrimaryText>
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
      >
        <SensitiveAmount
          amount={props.balance.amount}
          currency={props.userCurrency}
        />
      </AmountText>
    </Group>
  );
};

export default BalanceItemContent;
