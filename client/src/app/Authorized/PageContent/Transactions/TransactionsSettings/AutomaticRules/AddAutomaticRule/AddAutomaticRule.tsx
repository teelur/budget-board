import { Button, Group, Stack } from "@mantine/core";
import React from "react";
import { notifications } from "@mantine/notifications";
import {
  FieldToOperatorType,
  IRuleParameterEdit,
  Operators,
  OperatorTypes,
  TransactionFields,
} from "~/models/automaticRule";
import { getDefaultAction, hasEmptyTagAction } from "~/helpers/automaticRules";

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
    getDefaultAction(),
  ]);

  const { t } = useTranslation();
  const createAutomaticRuleMutation = useCreateAutomaticRuleMutation();
  const runAutomaticRuleMutation = useRunAutomaticRuleMutation();

  const hasValidActions = (): boolean => {
    if (!hasEmptyTagAction(actionItems)) {
      return true;
    }

    notifications.show({
      message: t("at_least_one_tag_required"),
      color: "var(--button-color-destructive)",
    });
    return false;
  };

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
            if (!hasValidActions()) {
              return;
            }

            const newAutomaticRule = {
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
            };

            createAutomaticRuleMutation.mutate(newAutomaticRule, {
              onSuccess: () => {
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
                setActionItems([getDefaultAction()]);
              },
            });
          }}
        >
          {t("add_rule")}
        </Button>
        <Button
          variant="outline"
          flex="1 1 auto"
          loading={runAutomaticRuleMutation.isPending}
          onClick={() => {
            if (!hasValidActions()) {
              return;
            }

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
