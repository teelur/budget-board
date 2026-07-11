import classes from "./DeletedTransactionsCard.module.css";

import { getDaysSinceDate } from "~/helpers/datetime";
import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { Undo2Icon } from "lucide-react";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
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
    <Card elevation={1}>
      <LoadingOverlay visible={restoreTransactionMutation.isPending} />
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
              restoreTransactionMutation.mutate(props.deletedTransaction.id)
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
