import classes from "./DeletedTransactionsCard.module.css";

import { Group, LoadingOverlay, Stack } from "@mantine/core";
import { ActionIcon, BodyText, Card } from "@teelur/budget-board-ui";
import { getDaysSinceDate } from "~/helpers/datetime";
import { ITransaction } from "~/models/transaction";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useRestoreTransactionMutation } from "~/hooks/mutations/transactions/useRestoreTransactionsMutation";

interface DeletedTransactionCardProps {
  deletedTransaction: ITransaction;
}

const DeletedTransactionsCard = (
  props: DeletedTransactionCardProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const restoreTransactionMutation = useRestoreTransactionMutation();

  return (
    <Card p="0.25rem">
      <Group justify="space-between" wrap="nowrap">
        <Stack gap={0}>
          <BodyText tone="primary" size="sm" fw={500}>
            {props.deletedTransaction.merchantName}
          </BodyText>
          <BodyText tone="secondary" size="xs">
            {t("days_since_deleted", {
              days: getDaysSinceDate(props.deletedTransaction.deleted!),
            })}
          </BodyText>
        </Stack>
        <Group className={classes.buttonGroup}>
          <ActionIcon
            variant="filled"
            color="primary"
            size="compact-sm"
            h="100%"
            loading={restoreTransactionMutation.isPending}
            onClick={() =>
              restoreTransactionMutation.mutate([props.deletedTransaction.id])
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
