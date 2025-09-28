import { Card, Group, Text } from "@mantine/core";
import dayjs from "dayjs";
import { convertNumberToCurrency } from "~/helpers/currency";

interface BalanceItemProps {
  dateTime: Date;
  amount: number;
  currency: string;
}

const BalanceItem = (props: BalanceItemProps) => {
  return (
    <Card radius="md" p="0.5rem">
      <Group justify="space-between" w="100%">
        <Text size="sm" fw={600} c="dimmed">
          {dayjs(props.dateTime).format("L LT")}
        </Text>
        <Text size="sm" fw={600} c={props.amount < 0 ? "red" : "green"}>
          {convertNumberToCurrency(props.amount, true, props.currency)}
        </Text>
      </Group>
    </Card>
  );
};

export default BalanceItem;
