import {
  ActionIcon,
  Card,
  Group,
  Select,
  Text,
  TextInput,
} from "@mantine/core";
import { Trash2Icon } from "lucide-react";
import {
  IRuleParameterCreateRequest,
  TransactionFields,
} from "~/models/automaticCategorizationRule";

export interface ActionItemProps {
  ruleParameter: IRuleParameterCreateRequest;
  setRuleParameter: (newParameter: IRuleParameterCreateRequest) => void;
  allowDelete: boolean;
  doDelete: (index: number) => void;
  index: number;
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
        {props.allowDelete && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              h="100%"
              size="sm"
              color="red"
              onClick={() => props.doDelete(props.index)}
            >
              <Trash2Icon size={16} />
            </ActionIcon>
          </Group>
        )}
      </Group>
    </Card>
  );
};

export default ActionItem;
