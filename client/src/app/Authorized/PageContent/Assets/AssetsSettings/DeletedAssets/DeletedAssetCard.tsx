import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { IAssetResponse } from "~/models/asset";
import ElevatedCard from "~/components/core/Card/ElevatedCard/ElevatedCard";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useRestoreAssetsMutation } from "~/hooks/mutations/assets/useRestoreAssetsMutation";

interface DeletedAssetCardProps {
  asset: IAssetResponse;
}

const DeletedAssetCard = (props: DeletedAssetCardProps): React.ReactNode => {
  const restoreAssetMutation = useRestoreAssetsMutation();

  return (
    <ElevatedCard>
      <LoadingOverlay visible={restoreAssetMutation.isPending} />
      <Group justify="space-between">
        <Stack gap={0}>
          <PrimaryText size="sm">{props.asset.name}</PrimaryText>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            h="100%"
            onClick={() => restoreAssetMutation.mutate(props.asset.id)}
          >
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </ElevatedCard>
  );
};

export default DeletedAssetCard;
