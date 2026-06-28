import { Button, Group, Stack } from "@mantine/core";
import React from "react";
import {
  ActionOperators,
  FieldToOperatorType,
  IRuleParameterEdit,
  Operators,
  OperatorTypes,
  TransactionFields,
} from "~/models/automaticRule";

import EditableAutomaticRuleContent from "../EditableAutomaticRuleContent/EditableAutomaticRuleContent";
import { useTranslation } from "react-i18next";
import { useCreateAutomaticRuleMutation } from "~/hooks/mutations/automaticRules/useCreateAutomaticRuleMutation";
import { useRunAutomaticRuleMutation } from "~/hooks/mutations/automaticRules/useRunAutomaticRuleMutation";

const AddAutomaticRule = (): React.ReactNode => {
  const defaultField =
    TransactionFields.find((field) => field.value === "merchant")?.value ?? "";

  const [conditionItems, setConditionItems] = React.useState<
    IRuleParameterEdit[]
  >([
    {
      field: defaultField,
      operator:
        Operators.filter((op) =>
          op.type.includes(
            FieldToOperatorType.get(defaultField) ?? OperatorTypes.STRING,
          ),
        )
          .map((op) => op.value)
          .at(0) ?? "",
      value: "",
    },
  ]);

  const [actionItems, setActionItems] = React.useState<IRuleParameterEdit[]>([
    {
      field: defaultField,
      operator: ActionOperators.at(0)!.value,
      value: "",
    },
  ]);

  const { t } = useTranslation();
  const createAutomaticRuleMutation = useCreateAutomaticRuleMutation();
  const runAutomaticRuleMutation = useRunAutomaticRuleMutation();

  return (
    <Stack gap="0.5rem">
      <EditableAutomaticRuleContent
        conditionItems={conditionItems}
        actionItems={actionItems}
        setConditionItems={setConditionItems}
        setActionItems={setActionItems}
      />
      <Group w="100%">
        <Button
          flex="1 1 auto"
          loading={createAutomaticRuleMutation.isPending}
          onClick={() => {
            createAutomaticRuleMutation.mutate({
              conditions: conditionItems.map((item) => ({
                field: item.field,
                operator: item.operator,
                value: item.value,
              })),
              actions: actionItems.map((item) => ({
                field: item.field,
                operator: item.operator,
                value: item.value,
              })),
            });

            // Reset to default
            setConditionItems([
              {
                field: defaultField,
                operator:
                  Operators.filter((op) =>
                    op.type.includes(
                      FieldToOperatorType.get(defaultField) ??
                        OperatorTypes.STRING,
                    ),
                  )
                    .map((op) => op.value)
                    .at(0) ?? "",
                value: "",
              },
            ]);
            setActionItems([
              {
                field: defaultField,
                operator:
                  Operators.find((op) =>
                    op.type.includes(
                      FieldToOperatorType.get(defaultField) ??
                        OperatorTypes.STRING,
                    ),
                  )?.value ?? "",
                value: "",
              },
            ]);
          }}
        >
          {t("add_rule")}
        </Button>
        <Button
          variant="outline"
          flex="1 1 auto"
          loading={runAutomaticRuleMutation.isPending}
          onClick={() => {
            runAutomaticRuleMutation.mutate({
              conditions: conditionItems.map((item) => ({
                field: item.field,
                operator: item.operator,
                value: item.value,
              })),
              actions: actionItems.map((item) => ({
                field: item.field,
                operator: item.operator,
                value: item.value,
              })),
            });
          }}
        >
          {t("run_rule")}
        </Button>
      </Group>
    </Stack>
  );
};

export default AddAutomaticRule;
