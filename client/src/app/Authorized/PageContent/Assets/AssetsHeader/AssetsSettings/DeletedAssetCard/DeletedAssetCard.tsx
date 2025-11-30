import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAssetResponse } from "~/models/asset";
import ElevatedCard from "~/components/Card/ElevatedCard/ElevatedCard";
import SurfacePrimaryText from "~/components/Text/Surface/SurfacePrimaryText/SurfacePrimaryText";

interface DeletedAssetCardProps {
  asset: IAssetResponse;
}

const DeletedAssetCard = (props: DeletedAssetCardProps): React.ReactNode => {
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doRestoreAsset = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/asset/restore`,
        method: "POST",
        params: { guid: props.asset.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  return (
    <ElevatedCard>
      <LoadingOverlay visible={doRestoreAsset.isPending} />
      <Group justify="space-between">
        <Stack gap={0}>
          <SurfacePrimaryText size="sm">{props.asset.name}</SurfacePrimaryText>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon h="100%" onClick={() => doRestoreAsset.mutate()}>
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </ElevatedCard>
  );
};

export default DeletedAssetCard;
