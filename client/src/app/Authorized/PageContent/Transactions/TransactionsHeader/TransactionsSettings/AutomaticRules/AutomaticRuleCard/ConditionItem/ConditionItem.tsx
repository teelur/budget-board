import { Card, Group, Text } from "@mantine/core";
import {
  IRuleParameterResponse,
  TransactionFields,
} from "~/models/automaticRule";

interface ConditionItemProps {
  condition: IRuleParameterResponse;
}

const ConditionItem = (props: ConditionItemProps) => {
  return (
    <Card
      bg="var(--mantine-color-card-alternate)"
      withBorder
      p="0.25rem"
      radius="md"
    >
      <Group gap="0.3rem">
        <Text fw={600} size="sm">
          {TransactionFields.find(
            (field) => field.value === props.condition.field
          )?.label ?? props.condition.field}
        </Text>
        <Text fw={600} size="sm">
          {props.condition.operator}
        </Text>
        <Text fw={600} size="sm">
          {props.condition.value}
        </Text>
      </Group>
    </Card>
  );
};

export default ConditionItem;
