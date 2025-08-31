import { Button, Stack, Group, Text, ActionIcon } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  FieldToOperatorType,
  IAutomaticCategorizationRuleRequest,
  IRuleParameterCreateRequest,
  Operators,
  TransactionFields,
} from "~/models/automaticCategorizationRule";
import { ICategoryResponse } from "~/models/category";
import { defaultTransactionCategories } from "~/models/transaction";
import ConditionItem from "./ConditionItem/ConditionItem";
import ActionItem from "./ActionItem/ActionItem";
import { PlusIcon } from "lucide-react";

const AddAutomaticRule = (): React.ReactNode => {
  const defaultField =
    TransactionFields.find((field) => field.value === "merchant")?.value ?? "";
  const [conditionItems, setConditionItems] = React.useState<
    IRuleParameterCreateRequest[]
  >([
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

  const [actionItems, setActionItems] = React.useState<
    IRuleParameterCreateRequest[]
  >([
    {
      field: defaultField,
      operator:
        Operators.find(
          (op) => op.type === FieldToOperatorType.get(defaultField)
        )?.value ?? "",
      value: "",
      type: "",
    },
  ]);

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

  const queryClient = useQueryClient();
  const doAddRule = useMutation({
    mutationFn: async (
      automaticCategorizationRule: IAutomaticCategorizationRuleRequest
    ) =>
      await request({
        url: "/api/automaticCategorizationRule",
        method: "POST",
        data: automaticCategorizationRule,
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({
        queryKey: ["automaticCategorizationRule"],
      }),
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  const addNewCondition = () => {
    setConditionItems((prev) => [
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
    setConditionItems((prev) => prev.filter((_, i) => i !== index));
  };

  const addNewAction = () => {
    setActionItems((prev) => [
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
    setActionItems((prev) => prev.filter((_, i) => i !== index));
  };

  return (
    <Stack gap="0.5rem">
      <Stack gap="0.5rem">
        <Group align="center" justify="space-between">
          <Text size="sm" fw={600}>
            If
          </Text>
          <ActionIcon size="sm" onClick={addNewCondition}>
            <PlusIcon size={16} />
          </ActionIcon>
        </Group>
        {conditionItems.map(
          (item: IRuleParameterCreateRequest, index: number) => (
            <ConditionItem
              key={index}
              ruleParameter={item}
              setRuleParameter={(newParameter) =>
                setConditionItems(
                  (
                    prev: IRuleParameterCreateRequest[]
                  ): IRuleParameterCreateRequest[] =>
                    prev.map((param, i) => (i === index ? newParameter : param))
                )
              }
              allowDelete={conditionItems.length > 1}
              doDelete={removeCondition}
              index={index}
              categories={transactionCategoriesWithCustom}
            />
          )
        )}
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
        {actionItems.map((item: IRuleParameterCreateRequest, index: number) => (
          <ActionItem
            key={index}
            ruleParameter={item}
            setRuleParameter={(newParameter) =>
              setActionItems(
                (
                  prev: IRuleParameterCreateRequest[]
                ): IRuleParameterCreateRequest[] =>
                  prev.map((param, i) => (i === index ? newParameter : param))
              )
            }
            allowDelete={actionItems.length > 1}
            doDelete={removeAction}
            index={index}
            categories={transactionCategoriesWithCustom}
          />
        ))}
      </Stack>
      <Button
        onClick={() =>
          doAddRule.mutate({ conditions: conditionItems, actions: actionItems })
        }
      >
        Add Rule
      </Button>
    </Stack>
  );
};

export default AddAutomaticRule;
