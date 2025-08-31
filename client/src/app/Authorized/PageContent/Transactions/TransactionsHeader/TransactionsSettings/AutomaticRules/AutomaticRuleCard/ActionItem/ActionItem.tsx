import { Card, Group, Text } from "@mantine/core";
import {
  IRuleParameterResponse,
  TransactionFields,
} from "~/models/automaticRule";

interface ActionItemProps {
  action: IRuleParameterResponse;
}

const ActionItem = (props: ActionItemProps) => {
  return (
    <Card
      bg="var(--mantine-color-card-alternate)"
      withBorder
      p="0.25rem"
      radius="md"
    >
      <Group gap="0.3rem">
        <Text fw={600} size="sm">
          Set
        </Text>
        <Text fw={600} size="sm">
          {TransactionFields.find((field) => field.value === props.action.field)
            ?.label ?? props.action.field}
        </Text>
        <Text fw={600} size="sm">
          to
        </Text>
        <Text fw={600} size="sm">
          {props.action.value}
        </Text>
      </Group>
    </Card>
  );
};

export default ActionItem;
