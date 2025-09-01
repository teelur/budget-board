import { Button, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";

import { translateAxiosError } from "~/helpers/requests";
import {
  FieldToOperatorType,
  IAutomaticRuleRequest,
  IRuleParameterEdit,
  Operators,
  OperatorTypes,
  TransactionFields,
} from "~/models/automaticRule";

import EditableAutomaticRuleContent from "../EditableAutomaticRuleContent/EditableAutomaticRuleContent";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";

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
            FieldToOperatorType.get(defaultField) ?? OperatorTypes.STRING
          )
        )
          .map((op) => op.value)
          .at(0) ?? "",
      value: "",
    },
  ]);

  const [actionItems, setActionItems] = React.useState<IRuleParameterEdit[]>([
    {
      field: defaultField,
      operator:
        Operators.find((op) =>
          op.type.includes(
            FieldToOperatorType.get(defaultField) ?? OperatorTypes.STRING
          )
        )?.value ?? "",
      value: "",
    },
  ]);

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doAddRule = useMutation({
    mutationFn: async (automaticRule: IAutomaticRuleRequest) =>
      await request({
        url: "/api/automaticRule",
        method: "POST",
        data: automaticRule,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["automaticRule"],
      });
      notifications.show({
        message: "Rule added successfully",
        color: "green",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  return (
    <Stack gap="0.5rem">
      <EditableAutomaticRuleContent
        conditionItems={conditionItems}
        actionItems={actionItems}
        setConditionItems={setConditionItems}
        setActionItems={setActionItems}
      />
      <Button
        loading={doAddRule.isPending}
        onClick={() => {
          doAddRule.mutate({
            conditions: conditionItems.map((item) => ({
              field: item.field,
              operator: item.operator,
              value: item.value,
              type: "",
            })),
            actions: actionItems.map((item) => ({
              field: item.field,
              operator: item.operator,
              value: item.value,
              type: "",
            })),
          });
          setConditionItems([
            {
              field: defaultField,
              operator:
                Operators.filter((op) =>
                  op.type.includes(
                    FieldToOperatorType.get(defaultField) ??
                      OperatorTypes.STRING
                  )
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
                      OperatorTypes.STRING
                  )
                )?.value ?? "",
              value: "",
            },
          ]);
        }}
      >
        Add Rule
      </Button>
    </Stack>
  );
};

export default AddAutomaticRule;
