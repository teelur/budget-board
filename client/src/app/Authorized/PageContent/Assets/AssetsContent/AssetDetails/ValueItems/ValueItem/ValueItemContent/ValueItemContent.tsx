import { ActionIcon, Group, Text } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IValueResponse } from "~/models/value";

interface ValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doSelect: () => void;
}

const ValueItemContent = (props: ValueItemContentProps): React.ReactNode => {
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <Text fw={600} size="md">
          {dayjs(props.value.dateTime).format("L")}
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
      <Text fw={600} size="md" c={props.value.amount < 0 ? "red" : "green"}>
        {convertNumberToCurrency(props.value.amount, true, props.userCurrency)}
      </Text>
    </Group>
  );
};

export default ValueItemContent;
