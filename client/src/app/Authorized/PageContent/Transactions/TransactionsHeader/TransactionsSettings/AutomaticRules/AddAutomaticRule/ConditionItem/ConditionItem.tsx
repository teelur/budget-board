import { ActionIcon, Card, Group, Select, TextInput } from "@mantine/core";
import { Trash2Icon } from "lucide-react";
import {
  FieldToOperatorType,
  IRuleParameterCreateRequest,
  Operators,
  TransactionFields,
} from "~/models/automaticCategorizationRule";

export interface ConditionItemProps {
  ruleParameter: IRuleParameterCreateRequest;
  setRuleParameter: (newParameter: IRuleParameterCreateRequest) => void;
  allowDelete?: boolean;
  doDelete?: (index: number) => void;
  index: number;
}

const ConditionItem = (props: ConditionItemProps): React.ReactNode => {
  return (
    <Card p="0.5rem" radius="md">
      <Group gap="0.5rem">
        <Select
          w="110px"
          data={TransactionFields.map((field) => field.label)}
          value={
            TransactionFields.find(
              (field) => field.value === props.ruleParameter.field
            )?.label ?? ""
          }
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              field:
                TransactionFields.find((field) => field.label === value)
                  ?.value ?? "",
              operator:
                Operators.filter(
                  (op) =>
                    op.type ===
                    FieldToOperatorType.get(
                      TransactionFields.find((field) => field.label === value)
                        ?.value ?? ""
                    )
                ).at(0)?.value ?? "",
              value: "",
            })
          }
          allowDeselect={false}
        />
        <Select
          w="120px"
          data={Operators.filter(
            (op) =>
              op.type === FieldToOperatorType.get(props.ruleParameter.field)
          ).map((op) => op.label)}
          value={
            Operators.find((op) => op.value === props.ruleParameter.operator)
              ?.label ?? ""
          }
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              operator: Operators.find((op) => op.label === value)?.value ?? "",
            })
          }
          allowDeselect={false}
        />
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
              color="red"
              size="sm"
              h="100%"
              onClick={() => props.doDelete?.(props.index)}
            >
              <Trash2Icon size={16} />
            </ActionIcon>
          </Group>
        )}
      </Group>
    </Card>
  );
};

export default ConditionItem;
