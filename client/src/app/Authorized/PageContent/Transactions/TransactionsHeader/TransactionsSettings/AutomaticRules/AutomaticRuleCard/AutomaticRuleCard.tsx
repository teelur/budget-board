import { ActionIcon, Card, Group, Stack, Text } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { TrashIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IAutomaticCategorizationRuleResponse } from "~/models/automaticCategorizationRule";
import ConditionItem from "./ConditionItem/ConditionItem";
import ActionItem from "./ActionItem/ActionItem";

interface CategorizationRuleCardProps {
  rule: IAutomaticCategorizationRuleResponse;
}

const AutomaticRuleCard = (props: CategorizationRuleCardProps) => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doDeleteCategorizationRule = useMutation({
    mutationFn: async (guid: string) => {
      await request({
        url: `/api/automaticCategorizationRule`,
        method: "DELETE",
        params: { guid },
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["automaticCategorizationRule"],
      });
    },
  });

  return (
    <Card p="0.5rem" radius="md">
      <Group justify="space-between">
        <Stack>
          <Group gap="0.5rem">
            <Text c="dimmed" fw={600} size="sm">
              If
            </Text>
            <Group gap="0.25rem">
              {(props.rule.conditions ?? []).map((condition) => (
                <ConditionItem key={condition.id} condition={condition} />
              ))}
            </Group>
          </Group>
          <Group gap="0.5rem">
            <Text c="dimmed" fw={600} size="sm">
              Then
            </Text>
            <Group gap="0.25rem">
              {(props.rule.actions ?? []).map((action) => (
                <ActionItem key={action.id} action={action} />
              ))}
            </Group>
          </Group>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="red"
            onClick={(e) => {
              e.stopPropagation();
              doDeleteCategorizationRule.mutate(props.rule.id);
            }}
            h="100%"
            loading={doDeleteCategorizationRule.isPending}
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default AutomaticRuleCard;
