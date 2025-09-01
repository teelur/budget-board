import { ActionIcon, Button, Card, Group, Stack, Text } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { PencilIcon, TrashIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import {
  IAutomaticRuleResponse,
  IAutomaticRuleUpdateRequest,
  IRuleParameterEdit,
} from "~/models/automaticRule";
import ConditionItem from "./ConditionItem/ConditionItem";
import ActionItem from "./ActionItem/ActionItem";
import EditableAutomaticRuleContent from "../EditableAutomaticRuleContent/EditableAutomaticRuleContent";
import { ICategoryResponse } from "~/models/category";
import { defaultTransactionCategories } from "~/models/transaction";

interface AutomaticRuleCardProps {
  rule: IAutomaticRuleResponse;
}

const AutomaticRuleCard = (props: AutomaticRuleCardProps) => {
  const [isSelected, setIsSelected] = React.useState(false);

  const [conditionItems, setConditionItems] = React.useState<
    IRuleParameterEdit[]
  >(props.rule.conditions ?? []);
  const [actionItems, setActionItems] = React.useState<IRuleParameterEdit[]>(
    props.rule.actions ?? []
  );

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

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async () => {
      const res = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doDeleteAutomaticRule = useMutation({
    mutationFn: async (guid: string) => {
      await request({
        url: `/api/automaticRule`,
        method: "DELETE",
        params: { guid },
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["automaticRule"],
      });
    },
  });

  const doUpdateAutomaticRule = useMutation({
    mutationFn: async (data: IAutomaticRuleUpdateRequest) => {
      await request({
        url: `/api/automaticRule`,
        method: "PUT",
        data,
      });
    },
    onSuccess: () => {
      queryClient.refetchQueries({
        queryKey: ["automaticRule"],
      });
      setIsSelected(false);
    },
  });

  if (isSelected) {
    return (
      <Card bg="var(--mantine-color-bg)" radius="md" withBorder>
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
              onClick={() => {
                doUpdateAutomaticRule.mutate({
                  id: props.rule.id,
                  conditions: conditionItems.map((item) => ({
                    id: item.id ?? "",
                    value: item.value,
                    field: item.field,
                    operator: item.operator,
                    type: "",
                  })),
                  actions: actionItems.map((item) => ({
                    id: item.id ?? "",
                    value: item.value,
                    field: item.field,
                    operator: item.operator,
                    type: "",
                  })),
                });
              }}
              loading={doUpdateAutomaticRule.isPending}
            >
              Save
            </Button>
            <Button
              flex="1 1 auto"
              variant="outline"
              onClick={() => setIsSelected(false)}
            >
              Cancel
            </Button>
          </Group>
        </Stack>
      </Card>
    );
  }

  return (
    <Card p="0.5rem" radius="md">
      <Group gap={0} justify="space-between" wrap="nowrap">
        <Stack>
          <Group gap="0.25rem">
            <Text c="dimmed" fw={600} size="sm" pr="0.5rem">
              If
            </Text>
            {(props.rule.conditions ?? []).map((condition) => (
              <ConditionItem
                key={condition.id}
                condition={condition}
                categories={transactionCategoriesWithCustom}
                currency={userSettingsQuery.data?.currency ?? ""}
              />
            ))}
          </Group>
          <Group gap="0.25rem">
            <Text c="dimmed" fw={600} size="sm" pr="0.5rem">
              Then
            </Text>
            {(props.rule.actions ?? []).map((action) => (
              <ActionItem
                key={action.id}
                action={action}
                categories={transactionCategoriesWithCustom}
                currency={userSettingsQuery.data?.currency ?? ""}
              />
            ))}
          </Group>
        </Stack>
        <Group style={{ alignSelf: "stretch" }} gap="0.5rem" wrap="nowrap">
          <ActionIcon
            onClick={() => {
              setConditionItems(props.rule.conditions ?? []);
              setActionItems(props.rule.actions ?? []);
              setIsSelected(true);
            }}
            h="100%"
          >
            <PencilIcon size="1rem" />
          </ActionIcon>
          <ActionIcon
            color="red"
            onClick={(e) => {
              e.stopPropagation();
              doDeleteAutomaticRule.mutate(props.rule.id);
            }}
            h="100%"
            loading={doDeleteAutomaticRule.isPending}
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default AutomaticRuleCard;
