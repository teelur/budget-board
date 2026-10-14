import { Group, Stack } from "@mantine/core";
import {
  Alert,
  BodyText,
  Button,
  Card,
  Checkbox,
  HeadingText,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import {
  InfoIcon,
  MoveLeftIcon,
  MoveRightIcon,
  RefreshCwIcon,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import DuplicateTransactionTable from "../ConfigureTransactions/DuplicateTransactionTable/DuplicateTransactionTable";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import {
  ITransaction,
  ITransactionImportDuplicateFieldAvailability,
  ITransactionImportDuplicateOptions,
  ITransactionImportTableData,
  TransactionImportDuplicateField,
} from "~/models/transaction";
import { filterImportedTransactionDuplicates } from "~/helpers/transactionImport";

interface DuplicateReviewProps {
  importedTransactions: ITransactionImportTableData[];
  duplicateOptions: ITransactionImportDuplicateOptions;
  setDuplicateOptions: React.Dispatch<
    React.SetStateAction<ITransactionImportDuplicateOptions>
  >;
  availableDuplicateFields: ITransactionImportDuplicateFieldAvailability;
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
  const [duplicateMatchFields, setDuplicateMatchFields] = React.useState<
    Map<number, TransactionImportDuplicateField[]>
  >(new Map());
  const [filteredTransactions, setFilteredTransactions] = React.useState<
    ITransactionImportTableData[]
  >(props.importedTransactions);

  const filterDuplicatesField = useField<boolean>({
    initialValue: props.duplicateOptions.filterDuplicates,
  });
  const filterByDateField = useField<boolean>({
    initialValue:
      props.duplicateOptions.filterByOptions.date &&
      props.availableDuplicateFields.date,
  });
  const filterByMerchantNameField = useField<boolean>({
    initialValue:
      props.duplicateOptions.filterByOptions.merchantName &&
      props.availableDuplicateFields.merchantName,
  });
  const filterByCategoryField = useField<boolean>({
    initialValue:
      props.duplicateOptions.filterByOptions.category &&
      props.availableDuplicateFields.category,
  });
  const filterByAmountField = useField<boolean>({
    initialValue:
      props.duplicateOptions.filterByOptions.amount &&
      props.availableDuplicateFields.amount,
  });
  const filterByAccountField = useField<boolean>({
    initialValue:
      props.duplicateOptions.filterByOptions.account &&
      props.availableDuplicateFields.account,
  });

  const filterFields: Array<{
    key: TransactionImportDuplicateField;
    label: string;
    field: ReturnType<typeof useField<boolean>>;
  }> = [
    { key: "date", label: t("date"), field: filterByDateField },
    {
      key: "merchantName",
      label: t("merchant_name"),
      field: filterByMerchantNameField,
    },
    { key: "category", label: t("category"), field: filterByCategoryField },
    { key: "amount", label: t("amount"), field: filterByAmountField },
    { key: "account", label: t("account"), field: filterByAccountField },
  ];

  const activeFilterFields = filterFields.filter(
    ({ key, field }) => props.availableDuplicateFields[key] && field.getValue(),
  );

  React.useEffect(() => {
    filterDuplicatesField.setValue(props.duplicateOptions.filterDuplicates);
    filterByDateField.setValue(
      props.duplicateOptions.filterByOptions.date &&
        props.availableDuplicateFields.date,
    );
    filterByMerchantNameField.setValue(
      props.duplicateOptions.filterByOptions.merchantName &&
        props.availableDuplicateFields.merchantName,
    );
    filterByCategoryField.setValue(
      props.duplicateOptions.filterByOptions.category &&
        props.availableDuplicateFields.category,
    );
    filterByAmountField.setValue(
      props.duplicateOptions.filterByOptions.amount &&
        props.availableDuplicateFields.amount,
    );
    filterByAccountField.setValue(
      props.duplicateOptions.filterByOptions.account &&
        props.availableDuplicateFields.account,
    );
  }, [props.importedTransactions, props.availableDuplicateFields]);

  React.useEffect(() => {
    props.setDuplicateOptions({
      filterDuplicates: filterDuplicatesField.getValue(),
      filterByOptions: {
        date: filterByDateField.getValue(),
        merchantName: filterByMerchantNameField.getValue(),
        category: filterByCategoryField.getValue(),
        amount: filterByAmountField.getValue(),
        account: filterByAccountField.getValue(),
      },
    });
  }, [
    filterDuplicatesField.getValue(),
    filterByDateField.getValue(),
    filterByMerchantNameField.getValue(),
    filterByCategoryField.getValue(),
    filterByAmountField.getValue(),
    filterByAccountField.getValue(),
  ]);

  React.useEffect(() => {
    if (transactionsQuery.isPending || transactionsQuery.isError) {
      return;
    }

    const result = filterImportedTransactionDuplicates(
      props.importedTransactions,
      transactionsQuery.data,
      transactionCategories,
      props.duplicateOptions,
      props.accountNameToAccountIdMap,
      props.availableDuplicateFields,
    );
    setDuplicateTransactions(result.duplicateTransactions);
    setDuplicateMatchFields(result.duplicateMatchFields);
    setFilteredTransactions(result.filteredTransactions);
  }, [
    props.importedTransactions,
    props.duplicateOptions,
    props.accountNameToAccountIdMap,
    transactionsQuery.data,
    transactionsQuery.isError,
    transactionsQuery.isPending,
    transactionCategories,
    props.availableDuplicateFields,
  ]);

  const restoreImportedTransaction = (uid: number) => {
    const restoredTransaction = Array.from(duplicateTransactions.keys()).find(
      (transaction) => transaction.uid === uid,
    );

    if (!restoredTransaction) {
      return;
    }

    setFilteredTransactions((previous) => {
      const restoredIndex = props.importedTransactions.findIndex(
        (transaction) => transaction.uid === uid,
      );
      const insertionIndex = previous.filter(
        (transaction) =>
          props.importedTransactions.findIndex(
            (importedTransaction) =>
              importedTransaction.uid === transaction.uid,
          ) < restoredIndex,
      ).length;
      const next = [...previous];
      next.splice(insertionIndex, 0, restoredTransaction);
      return next;
    });
    setDuplicateTransactions((previous) => {
      const next = new Map(previous);
      next.delete(restoredTransaction);
      return next;
    });
  };

  const getAlerts = (): React.ReactNode[] => {
    const alerts: React.ReactNode[] = [];

    if (transactionsQuery.isError) {
      alerts.push(
        <Stack key="transaction-query-error" gap="0.5rem">
          <Alert variant="light" color="error" icon={<InfoIcon />}>
            {t("duplicate_review_error_message")}
          </Alert>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            rightSection={<RefreshCwIcon size={16} />}
            loading={transactionsQuery.isRefetching}
            onClick={() => transactionsQuery.refetch()}
          >
            {t("retry")}
          </Button>
        </Stack>,
      );
    }

    if (!filterDuplicatesField.getValue()) {
      alerts.push(
        <Alert
          key="duplicate-filter-disabled"
          variant="light"
          color="warning"
          icon={<InfoIcon />}
        >
          {t("duplicate_review_filter_disabled_message")}
        </Alert>,
      );

      // The remaining alerts are not relevant if the duplicate filter is disabled
      return alerts;
    }

    if (filteredTransactions.length === 0) {
      alerts.push(
        <Alert
          key="no-transactions"
          variant="light"
          color="error"
          icon={<InfoIcon />}
        >
          {t("duplicate_review_no_transactions_message")}
        </Alert>,
      );
    }

    if (activeFilterFields.length === 0) {
      alerts.push(
        <Alert
          key="no-filter-criteria"
          variant="light"
          color="error"
          icon={<InfoIcon />}
        >
          {t("duplicate_filter_no_criteria_message")}
        </Alert>,
      );
    }

    return alerts;
  };

  const getTransactionsContent = () => {
    if (transactionsQuery.isPending) {
      return (
        <BodyText tone="secondary" size="sm">
          {t("duplicate_review_loading_message")}
        </BodyText>
      );
    }
    if (transactionsQuery.isError) {
      return (
        <BodyText tone="muted" size="sm">
          {t("error")}
        </BodyText>
      );
    }
    return (
      <Stack gap="0.5rem">
        <Group justify="space-between" gap="1rem">
          <BodyText tone="secondary" size="sm">
            {t("duplicate_review_imported_count", {
              count: props.importedTransactions.length,
            })}
          </BodyText>
          <BodyText tone="secondary" size="sm">
            {t("duplicate_review_duplicates_count", {
              count: duplicateTransactions.size,
            })}
          </BodyText>
          <BodyText tone="secondary" size="sm">
            {t("duplicate_review_ready_count", {
              count: filteredTransactions.length,
            })}
          </BodyText>
        </Group>
        {filterDuplicatesField.getValue() && activeFilterFields.length > 0 && (
          <BodyText size="xs">
            {t("duplicate_review_matching_on", {
              fields: activeFilterFields.map(({ label }) => label).join(", "),
            })}
          </BodyText>
        )}
        {duplicateTransactions.size > 0 && (
          <DuplicateTransactionTable
            tableData={duplicateTransactions}
            matchFields={duplicateMatchFields}
            restoreTransaction={restoreImportedTransaction}
          />
        )}
        {filterDuplicatesField.getValue() &&
          activeFilterFields.length > 0 &&
          duplicateTransactions.size === 0 && (
            <Alert variant="light" color="success" icon={<InfoIcon />}>
              {t("duplicate_review_no_duplicates_message")}
            </Alert>
          )}
      </Stack>
    );
  };

  return (
    <Stack gap="0.75rem" w={800} maw="100%" mx="auto">
      <Stack gap="0.25rem">
        <HeadingText tone="heading" level={5}>
          {t("duplicate_review")}
        </HeadingText>
        <BodyText tone="secondary" size="sm">
          {t("duplicate_review_description")}
        </BodyText>
        {getAlerts()}
      </Stack>
      <Checkbox
        checked={filterDuplicatesField.getValue()}
        onChange={(event) =>
          filterDuplicatesField.setValue(event.currentTarget.checked)
        }
        label={t("filter_duplicates")}
      />
      {filterDuplicatesField.getValue() && (
        <Card>
          <Stack gap="0.75rem">
            {filterDuplicatesField.getValue() && (
              <Stack gap="0.5rem">
                <BodyText tone="primary" size="sm">
                  {t("filter_duplicates_by")}
                </BodyText>
                <Group gap="1rem">
                  {filterFields.map(({ key, label, field }) => (
                    <Checkbox
                      key={key}
                      checked={field.getValue()}
                      disabled={!props.availableDuplicateFields[key]}
                      onChange={(event) =>
                        field.setValue(event.currentTarget.checked)
                      }
                      label={t(label)}
                    />
                  ))}
                </Group>
                {filterFields.some(
                  ({ key }) => !props.availableDuplicateFields[key],
                ) && (
                  <BodyText tone="secondary" size="xs">
                    {t("duplicate_review_unavailable_fields_message")}
                  </BodyText>
                )}
              </Stack>
            )}
          </Stack>
        </Card>
      )}
      {getTransactionsContent()}
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
          onClick={props.goBackToPreviousDialog}
        >
          {t("back")}
        </Button>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          rightSection={<MoveRightIcon size={16} />}
          disabled={
            transactionsQuery.isPending ||
            transactionsQuery.isError ||
            filteredTransactions.length === 0
          }
          loading={
            transactionsQuery.isPending || transactionsQuery.isRefetching
          }
          onClick={() => props.advanceToNextDialog(filteredTransactions)}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default DuplicateReview;
