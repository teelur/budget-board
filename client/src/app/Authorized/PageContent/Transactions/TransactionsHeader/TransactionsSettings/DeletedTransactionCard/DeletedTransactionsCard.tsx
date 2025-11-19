import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import classes from "./DeletedTransactionsCard.module.css";

import { getDaysSinceDate } from "~/helpers/datetime";
import {
  ActionIcon,
  Card,
  Group,
  LoadingOverlay,
  Stack,
  Text,
} from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";

interface DeletedTransactionCardProps {
  deletedTransaction: ITransaction;
}

const DeletedTransactionsCard = (
  props: DeletedTransactionCardProps
): React.ReactNode => {
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doRestoreTransaction = useMutation({
    mutationFn: async (id: string) => {
      return await request({
        url: "/api/transaction/restore",
        method: "POST",
        params: { guid: id },
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  return (
    <Card padding="0.5rem" radius="md">
      <LoadingOverlay visible={doRestoreTransaction.isPending} />
      <Group justify="space-between" wrap="nowrap">
        <Stack gap={0}>
          <Text fw={600} size="md">
            {props.deletedTransaction.merchantName}
          </Text>
          <Text c="dimmed" fw={600} size="sm">{`${getDaysSinceDate(
            props.deletedTransaction.deleted!
          )} days since deleted`}</Text>
        </Stack>
        <Group className={classes.buttonGroup}>
          <ActionIcon
            h="100%"
            onClick={() =>
              doRestoreTransaction.mutate(props.deletedTransaction.id)
            }
          >
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default DeletedTransactionsCard;
