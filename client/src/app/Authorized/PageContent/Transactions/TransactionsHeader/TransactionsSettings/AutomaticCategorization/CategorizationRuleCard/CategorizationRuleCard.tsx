import { ActionIcon, Card, Group, Stack, Text } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { TrashIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";

interface CategorizationRuleCardProps {
  id: string;
  rule: string;
  category: string;
}

const CategorizationRuleCard = (props: CategorizationRuleCardProps) => {
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
          <Group>
            <Text c="dimmed" fw={600} size="sm">
              Rule
            </Text>
            <Text fw={600}>{props.rule}</Text>
          </Group>
          <Group>
            <Text c="dimmed" fw={600} size="sm">
              Category
            </Text>
            <Text fw={600}>{props.category}</Text>
          </Group>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="red"
            onClick={(e) => {
              e.stopPropagation();
              doDeleteCategorizationRule.mutate(props.id);
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

export default CategorizationRuleCard;
