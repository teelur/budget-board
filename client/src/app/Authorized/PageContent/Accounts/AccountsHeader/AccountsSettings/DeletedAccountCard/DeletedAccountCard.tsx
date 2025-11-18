import {
  ActionIcon,
  Badge,
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
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";

interface DeletedAccountCardProps {
  account: IAccountResponse;
  institutionName?: string;
}

const DeletedAccountCard = (
  props: DeletedAccountCardProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doRestoreAccount = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/account/restore`,
        method: "POST",
        params: { guid: props.account.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  return (
    <Card p="0.5rem">
      <LoadingOverlay visible={doRestoreAccount.isPending} />
      <Group justify="space-between" wrap="nowrap">
        <Group gap="0.5rem">
          <Stack gap={0}>
            <Text fw={600} size="sm">
              {props.account.name}
            </Text>
            <Text size="xs" c="dimmed" fw={600}>
              {props.institutionName ?? "Unknown Institution"}
            </Text>
          </Stack>
          {props.account.syncID !== null && <Badge bg="blue">SimpleFIN</Badge>}
        </Group>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon h="100%" onClick={() => doRestoreAccount.mutate()}>
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default DeletedAccountCard;
