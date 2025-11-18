import {
  ActionIcon,
  Card,
  Group,
  LoadingOverlay,
  Stack,
  Text,
} from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAssetResponse } from "~/models/asset";

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
    <Card p="0.5rem">
      <LoadingOverlay visible={doRestoreAsset.isPending} />
      <Group justify="space-between">
        <Stack gap={0}>
          <Text fw={600} size="sm">
            {props.asset.name}
          </Text>
        </Stack>

        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon h="100%" onClick={() => doRestoreAsset.mutate()}>
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default DeletedAssetCard;
