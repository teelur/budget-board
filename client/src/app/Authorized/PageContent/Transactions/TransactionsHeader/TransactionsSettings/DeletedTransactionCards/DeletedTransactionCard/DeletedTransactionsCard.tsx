import classes from "./DeletedTransactionsCard.module.css";

import { getDaysSinceDate } from "~/helpers/datetime";
import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

interface DeletedTransactionCardProps {
  deletedTransaction: ITransaction;
}

const DeletedTransactionsCard = (
  props: DeletedTransactionCardProps
): React.ReactNode => {
  const { t } = useTranslation();
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
      notifications.show({
        color: "var(--button-color-success)",
        message: t("transaction_restored_successfully_message"),
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Card elevation={2}>
      <LoadingOverlay visible={doRestoreTransaction.isPending} />
      <Group justify="space-between" wrap="nowrap">
        <Stack gap={0}>
          <PrimaryText size="md">
            {props.deletedTransaction.merchantName}
          </PrimaryText>
          <DimmedText size="sm">
            {t("days_since_deleted", {
              days: getDaysSinceDate(props.deletedTransaction.deleted!),
            })}
          </DimmedText>
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
