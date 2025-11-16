import { ActionIcon, Group, Text } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
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
        <Text fw={600} size="md">
          {dayjs(props.balance.dateTime).format("L")}
        </Text>
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
      <Text fw={600} size="md" c={props.balance.amount < 0 ? "red" : "green"}>
        {convertNumberToCurrency(
          props.balance.amount,
          true,
          props.userCurrency
        )}
      </Text>
    </Group>
  );
};

export default BalanceItemContent;
