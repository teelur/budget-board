import { Button, Group, Stack } from "@mantine/core";
import React from "react";
import { MoveLeftIcon, MoveRightIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DuplicateTransactionTable from "../ConfigureTransactions/DuplicateTransactionTable/DuplicateTransactionTable";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import {
  ITransaction,
  ITransactionImportDuplicateOptions,
  ITransactionImportTableData,
} from "~/models/transaction";
import { filterImportedTransactionDuplicates } from "~/helpers/transactionImport";

interface DuplicateReviewProps {
  importedTransactions: ITransactionImportTableData[];
  duplicateOptions: ITransactionImportDuplicateOptions;
  accountNameToAccountIdMap: Map<string, string>;
  goBackToPreviousDialog: () => void;
  advanceToNextDialog: (data: ITransactionImportTableData[]) => void;
}

const DuplicateReview = (props: DuplicateReviewProps): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const transactionsQuery = useTransactionsQuery({
    includeHiddenCategory: true,
  });
  const [duplicateTransactions, setDuplicateTransactions] = React.useState<
    Map<ITransactionImportTableData, ITransaction>
  >(new Map());
  const [filteredTransactions, setFilteredTransactions] = React.useState<
    ITransactionImportTableData[]
  >(props.importedTransactions);

  React.useEffect(() => {
    if (transactionsQuery.isPending) {
      return;
    }

    const result = filterImportedTransactionDuplicates(
      props.importedTransactions,
      transactionsQuery.data,
      transactionCategories,
      props.duplicateOptions,
      props.accountNameToAccountIdMap,
    );
    setDuplicateTransactions(result.duplicateTransactions);
    setFilteredTransactions(result.filteredTransactions);
  }, [
    props.importedTransactions,
    props.duplicateOptions,
    props.accountNameToAccountIdMap,
    transactionsQuery.data,
    transactionsQuery.isPending,
    transactionCategories,
  ]);

  const restoreImportedTransaction = (uid: number) => {
    const restoredTransaction = Array.from(duplicateTransactions.keys()).find(
      (transaction) => transaction.uid === uid,
    );

    if (!restoredTransaction) {
      return;
    }

    setFilteredTransactions((previous) => [...previous, restoredTransaction]);
    setDuplicateTransactions((previous) => {
      const next = new Map(previous);
      next.delete(restoredTransaction);
      return next;
    });
  };

  return (
    <Stack gap="0.5rem" w="auto" maw="100%" mx="auto">
      {duplicateTransactions.size > 0 && (
        <DuplicateTransactionTable
          tableData={duplicateTransactions}
          restoreTransaction={restoreImportedTransaction}
        />
      )}
      <PrimaryText size="sm">
        {t("import_n_transactions", { n: filteredTransactions.length })}
      </PrimaryText>
      <Group w="100%">
        <Button
          flex="1 1 auto"
          onClick={props.goBackToPreviousDialog}
          leftSection={<MoveLeftIcon size={16} />}
        >
          {t("back")}
        </Button>
        <Button
          flex="1 1 auto"
          disabled={transactionsQuery.isPending}
          loading={transactionsQuery.isPending}
          onClick={() => props.advanceToNextDialog(filteredTransactions)}
          rightSection={<MoveRightIcon size={16} />}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default DuplicateReview;
