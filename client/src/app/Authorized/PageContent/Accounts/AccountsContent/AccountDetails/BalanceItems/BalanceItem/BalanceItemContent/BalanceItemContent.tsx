import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { IBalanceResponse } from "~/models/balance";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface BalanceItemContentProps {
  balance: IBalanceResponse;
  userCurrency: string;
  doSelect: () => void;
}

const BalanceItemContent = (
  props: BalanceItemContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat } = useLocale();

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">
          {dayjs(props.balance.date).format(longDateFormat)}
        </PrimaryText>
        <ActionIcon
          variant="transparent"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.doSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <StatusText amount={props.balance.amount} size="md">
        <SensitiveAmount
          amount={props.balance.amount}
          currency={props.userCurrency}
        />
      </StatusText>
    </Group>
  );
};

export default BalanceItemContent;
