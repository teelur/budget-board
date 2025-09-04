import { Card, Group, Text } from "@mantine/core";
import { getFormattedValue } from "~/helpers/automaticRules";
import {
  ActionOperators,
  IRuleParameterResponse,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";

interface ActionItemProps {
  action: IRuleParameterResponse;
  categories: ICategory[];
  currency: string;
}

const ActionItem = (props: ActionItemProps) => {
  const getCardContent = (): React.ReactNode => {
    if (props.action.operator === "delete") {
      return (
        <Text fw={600} size="sm">
          the transaction
        </Text>
      );
    } else if (props.action.operator === "set") {
      return (
        <>
          <Text fw={600} size="sm">
            {TransactionFields.find(
              (field) => field.value === props.action.field
            )?.label ?? props.action.field}
          </Text>
          <Text fw={600} size="sm">
            to
          </Text>
          <Text fw={600} size="sm">
            {getFormattedValue(
              props.action.field,
              props.action.value,
              props.currency,
              props.categories
            )}
          </Text>
        </>
      );
    }
    return null;
  };

  return (
    <Card
      bg="var(--mantine-color-card-alternate)"
      withBorder
      p="0.25rem"
      radius="md"
    >
      <Group gap="0.3rem">
        <Text fw={600} size="sm">
          {ActionOperators.find((op) => op.value === props.action.operator)
            ?.label ?? props.action.operator}
        </Text>
        {getCardContent()}
      </Group>
    </Card>
  );
};

export default ActionItem;
