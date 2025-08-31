import { Card, Group, Select, Text, TextInput } from "@mantine/core";
import {
  IRuleParameterCreateRequest,
  TransactionFields,
} from "~/models/automaticCategorizationRule";

export interface ActionItemProps {
  ruleParameter: IRuleParameterCreateRequest;
  setRuleParameter: (newParameter: IRuleParameterCreateRequest) => void;
}

const ActionItem = (props: ActionItemProps) => {
  return (
    <Card p="0.5rem" radius="md">
      <Group gap="0.5rem">
        <Text size="sm" fw={600}>
          Set
        </Text>
        <Select
          w="110px"
          data={TransactionFields.map((field) => field.label)}
          value={props.ruleParameter.field}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              field: value ?? "",
            })
          }
        />
        <Text size="sm" fw={600}>
          to
        </Text>
        <TextInput
          flex="1 1 auto"
          value={props.ruleParameter.value}
          onChange={(event) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: event.currentTarget.value,
            })
          }
        />
      </Group>
    </Card>
  );
};

export default ActionItem;
