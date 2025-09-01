import { Card, Group, Text } from "@mantine/core";
import { getFormattedValue } from "~/helpers/automaticRules";
import {
  IRuleParameterResponse,
  Operators,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";

interface ConditionItemProps {
  condition: IRuleParameterResponse;
  categories: ICategory[];
  currency: string;
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
          {Operators.find((op) => op.value === props.condition.operator)
            ?.label ?? props.condition.operator}
        </Text>
        <Text fw={600} size="sm">
          {getFormattedValue(
            props.condition.field,
            props.condition.value,
            props.currency,
            props.categories
          )}
        </Text>
      </Group>
    </Card>
  );
};

export default ConditionItem;
