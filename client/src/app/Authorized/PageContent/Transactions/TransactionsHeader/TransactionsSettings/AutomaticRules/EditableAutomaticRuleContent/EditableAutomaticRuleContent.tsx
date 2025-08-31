import { ActionIcon, Group, Stack, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { PlusIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import {
  FieldToOperatorType,
  IRuleParameterEdit,
  Operators,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategoryResponse } from "~/models/category";
import { defaultTransactionCategories } from "~/models/transaction";
import ActionItem from "./ActionItem/ActionItem";
import ConditionItem from "./ConditionItem/ConditionItem";

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

  const { request } = React.useContext<any>(AuthContext);

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

  const addNewCondition = () => {
    props.setConditionItems((prev) => [
      ...prev,
      {
        field: defaultField,
        operator:
          Operators.filter(
            (op) => op.type === FieldToOperatorType.get(defaultField)
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
        field: TransactionFields.at(0)?.value ?? "",
        operator:
          Operators.find(
            (op) => op.type === FieldToOperatorType.get(defaultField)
          )?.value ?? "",
        value: "",
        type: "",
      },
    ]);
  };

  const removeAction = (index: number) => {
    props.setActionItems((prev) => prev.filter((_, i) => i !== index));
  };

  return (
    <Stack>
      <Stack gap="0.5rem">
        <Group align="center" justify="space-between">
          <Text size="sm" fw={600}>
            If
          </Text>
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
            categories={transactionCategoriesWithCustom}
          />
        ))}
      </Stack>
      <Stack gap="0.5rem">
        <Group align="center" justify="space-between">
          <Text size="sm" fw={600}>
            Then
          </Text>
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
            categories={transactionCategoriesWithCustom}
          />
        ))}
      </Stack>
    </Stack>
  );
};

export default EditableAutomaticRuleContent;
