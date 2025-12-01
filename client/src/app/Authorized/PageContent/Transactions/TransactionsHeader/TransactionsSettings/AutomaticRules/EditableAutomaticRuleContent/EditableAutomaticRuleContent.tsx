import { ActionIcon, Group, Stack } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import React from "react";
import {
  ActionOperators,
  FieldToOperatorType,
  IRuleParameterEdit,
  Operators,
  OperatorTypes,
  TransactionFields,
} from "~/models/automaticRule";
import ActionItem from "./ActionItem/ActionItem";
import ConditionItem from "./ConditionItem/ConditionItem";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

interface EditableAutomaticRuleContentProps {
  conditionItems: IRuleParameterEdit[];
  actionItems: IRuleParameterEdit[];
  setConditionItems: React.Dispatch<React.SetStateAction<IRuleParameterEdit[]>>;
  setActionItems: React.Dispatch<React.SetStateAction<IRuleParameterEdit[]>>;
}

const EditableAutomaticRuleContent = (
  props: EditableAutomaticRuleContentProps
): React.ReactNode => {
  const defaultField =
    TransactionFields.find((field) => field.value === "merchant")?.value ?? "";

  const { transactionCategories } = useTransactionCategories();

  const addNewCondition = () => {
    props.setConditionItems((prev) => [
      ...prev,
      {
        field: defaultField,
        operator:
          Operators.filter((op) =>
            op.type.includes(
              FieldToOperatorType.get(defaultField) ?? OperatorTypes.STRING
            )
          )
            .map((op) => op.value)
            .at(0) ?? "",
        value: "",
        type: "",
      },
    ]);
  };

  const removeCondition = (index: number) => {
    props.setConditionItems((prev) => prev.filter((_, i) => i !== index));
  };

  const addNewAction = () => {
    props.setActionItems((prev) => [
      ...prev,
      {
        field: TransactionFields.at(0)!.value,
        operator: ActionOperators.at(0)!.value,
        value: "",
        type: "",
      },
    ]);
  };

  const removeAction = (index: number) => {
    props.setActionItems((prev) => prev.filter((_, i) => i !== index));
  };

  return (
    <Stack gap="0.5rem">
      <Stack gap="0.5rem">
        <Group align="center" justify="space-between">
          <PrimaryText size="sm">If</PrimaryText>
          <ActionIcon size="sm" onClick={addNewCondition}>
            <PlusIcon size={16} />
          </ActionIcon>
        </Group>
        {props.conditionItems.map((item: IRuleParameterEdit, index: number) => (
          <ConditionItem
            key={index}
            ruleParameter={item}
            setRuleParameter={(newParameter) =>
              props.setConditionItems(
                (prev: IRuleParameterEdit[]): IRuleParameterEdit[] =>
                  prev.map((param, i) => (i === index ? newParameter : param))
              )
            }
            allowDelete={props.conditionItems.length > 1}
            doDelete={removeCondition}
            index={index}
            categories={transactionCategories}
          />
        ))}
      </Stack>
      <Stack gap="0.5rem">
        <Group align="center" justify="space-between">
          <PrimaryText size="sm">Then</PrimaryText>
          <ActionIcon size="sm" onClick={addNewAction}>
            <PlusIcon size={16} />
          </ActionIcon>
        </Group>
        {props.actionItems.map((item: IRuleParameterEdit, index: number) => (
          <ActionItem
            key={index}
            ruleParameter={item}
            setRuleParameter={(newParameter) =>
              props.setActionItems(
                (prev: IRuleParameterEdit[]): IRuleParameterEdit[] =>
                  prev.map((param, i) => (i === index ? newParameter : param))
              )
            }
            allowDelete={props.actionItems.length > 1}
            doDelete={removeAction}
            index={index}
            categories={transactionCategories}
          />
        ))}
      </Stack>
    </Stack>
  );
};

export default EditableAutomaticRuleContent;
