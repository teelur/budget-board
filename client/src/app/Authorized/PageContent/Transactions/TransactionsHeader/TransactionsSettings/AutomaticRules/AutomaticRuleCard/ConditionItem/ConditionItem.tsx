import { Badge, Group } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
    <Card p="0.25rem" shadow="xs" elevation={1}>
      <Group gap="0.3rem">
        <Badge bg="purple" size="sm">
          {TransactionFields.find(
            (field) => field.value === props.condition.field
          )?.label ?? props.condition.field}
        </Badge>
        <PrimaryText size="sm">
          {Operators.find((op) => op.value === props.condition.operator)
            ?.label ?? props.condition.operator}
        </PrimaryText>
        <Badge size="sm">
          {getFormattedValue(
            props.condition.field,
            props.condition.value,
            props.currency,
            props.categories
          )}
        </Badge>
      </Group>
    </Card>
  );
};

export default ConditionItem;
