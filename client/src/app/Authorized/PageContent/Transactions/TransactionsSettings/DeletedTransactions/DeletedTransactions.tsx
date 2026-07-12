import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import { getDeletedTransactions } from "~/helpers/transactions";
import DeletedTransactionCards from "./DeletedTransactionCards/DeletedTransactionCards";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const Deleted = (): React.ReactNode => {
  const { t } = useTranslation();
  const transactionsQuery = useTransactionsQuery({
    includeHidden: true,
    includeDeleted: true,
  });

  const deletedTransactions = getDeletedTransactions(
    [...(transactionsQuery.data ?? [])].sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    ),
  );

  const getDeletedTransactionContent = (): React.ReactNode => {
    if (transactionsQuery.isPending) {
      return <Skeleton height={63} radius="md" />;
    }

    if (deletedTransactions.length === 0) {
      return (
        <Stack align="center" p="1rem">
          <DimmedText size="sm">{t("no_deleted_transactions")}</DimmedText>
        </Stack>
      );
    }

    return <DeletedTransactionCards transactions={deletedTransactions} />;
  };

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">
        {t("view_and_restore_deleted_transactions")}
      </DimmedText>
      {getDeletedTransactionContent()}
    </Stack>
  );
};

export default Deleted;
