import { Card, Group, Text } from "@mantine/core";
import dayjs from "dayjs";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IValueResponse } from "~/models/value";

interface ValueItemProps {
  value: IValueResponse;
  userCurrency: string;
}

const ValueItem = (props: ValueItemProps) => {
  return (
    <Card radius="md" p="0.5rem">
      <Group justify="space-between" align="center">
        <Text fw={600} size="md">
          {dayjs(props.value.dateTime).format("L")}
        </Text>
        <Text fw={600} size="md" c={props.value.amount < 0 ? "red" : "green"}>
          {convertNumberToCurrency(
            props.value.amount,
            true,
            props.userCurrency
          )}
        </Text>
      </Group>
    </Card>
  );
};

export default ValueItem;
