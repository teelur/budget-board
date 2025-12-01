import { ActionIcon, Group } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/Text/StatusText/StatusText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IBalanceResponse } from "~/models/balance";

interface BalanceItemContentProps {
  balance: IBalanceResponse;
  userCurrency: string;
  doSelect: () => void;
}

const BalanceItemContent = (
  props: BalanceItemContentProps
): React.ReactNode => {
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">
          {dayjs(props.balance.dateTime).format("L")}
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
        {convertNumberToCurrency(
          props.balance.amount,
          true,
          props.userCurrency
        )}
      </StatusText>
    </Group>
  );
};

export default BalanceItemContent;
