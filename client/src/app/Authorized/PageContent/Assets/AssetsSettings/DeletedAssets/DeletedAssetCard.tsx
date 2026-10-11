import { Group, Stack } from "@mantine/core";
import { ActionIcon, BodyText, Card } from "@teelur/budget-board-ui";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { IAssetResponse } from "~/models/asset";
import { useRestoreAssetsMutation } from "~/hooks/mutations/assets/useRestoreAssetsMutation";

interface DeletedAssetCardProps {
  asset: IAssetResponse;
}

const DeletedAssetCard = (props: DeletedAssetCardProps): React.ReactNode => {
  const restoreAssetMutation = useRestoreAssetsMutation();

  return (
    <Card p="0.25rem">
      <Group justify="space-between">
        <Stack gap={0}>
          <BodyText tone="primary" size="sm" fw={500}>
            {props.asset.name}
          </BodyText>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            variant="filled"
            color="primary"
            size="compact-xs"
            h="100%"
            loading={restoreAssetMutation.isPending}
            onClick={() => restoreAssetMutation.mutate(props.asset.id)}
          >
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default DeletedAssetCard;
