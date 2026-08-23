import { Alert, Button, Checkbox, Divider, Group, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { InfoIcon, MoveLeftIcon, MoveRightIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
    if (transactionsQuery.isPending) {
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

  return (
    <Stack gap="0.75rem" w={800} maw="100%" mx="auto">
      <Stack gap="0.25rem">
        <Divider label={t("duplicate_review")} labelPosition="center" />
        <PrimaryText size="sm">{t("duplicate_review_description")}</PrimaryText>
      </Stack>
      <Card elevation={1}>
        <Stack gap="0.75rem">
          <Checkbox
            checked={filterDuplicatesField.getValue()}
            onChange={(event) =>
              filterDuplicatesField.setValue(event.currentTarget.checked)
            }
            label={
              <PrimaryText size="sm">{t("filter_duplicates")}</PrimaryText>
            }
          />
          <DimmedText size="xs">
            {t("filter_duplicates_description")}
          </DimmedText>
          {filterDuplicatesField.getValue() && (
            <Stack gap="0.5rem">
              <PrimaryText size="sm">{t("filter_duplicates_by")}</PrimaryText>
              <Group gap="1rem">
                {filterFields.map(({ key, label, field }) => (
                  <Checkbox
                    key={key}
                    checked={field.getValue()}
                    disabled={!props.availableDuplicateFields[key]}
                    onChange={(event) =>
                      field.setValue(event.currentTarget.checked)
                    }
                    label={<PrimaryText size="sm">{label}</PrimaryText>}
                  />
                ))}
              </Group>
              {filterFields.some(
                ({ key }) => !props.availableDuplicateFields[key],
              ) && (
                <DimmedText size="xs">
                  {t("duplicate_review_unavailable_fields_message")}
                </DimmedText>
              )}
              {activeFilterFields.length === 0 && (
                <Alert
                  variant="outline"
                  color="var(--text-color-status-warn)"
                  icon={<InfoIcon />}
                >
                  <PrimaryText size="sm">
                    {t("duplicate_filter_no_criteria_message")}
                  </PrimaryText>
                </Alert>
              )}
            </Stack>
          )}
        </Stack>
      </Card>
      {transactionsQuery.isPending ? (
        <PrimaryText size="sm">
          {t("duplicate_review_loading_message")}
        </PrimaryText>
      ) : (
        <Stack gap="0.5rem">
          <Group justify="space-between" gap="1rem">
            <PrimaryText size="sm">
              {t("duplicate_review_imported_count", {
                count: props.importedTransactions.length,
              })}
            </PrimaryText>
            <PrimaryText size="sm">
              {t("duplicate_review_duplicates_count", {
                count: duplicateTransactions.size,
              })}
            </PrimaryText>
            <PrimaryText size="sm">
              {t("duplicate_review_ready_count", {
                count: filteredTransactions.length,
              })}
            </PrimaryText>
          </Group>
          {filterDuplicatesField.getValue() &&
            activeFilterFields.length > 0 && (
              <PrimaryText size="xs">
                {t("duplicate_review_matching_on", {
                  fields: activeFilterFields
                    .map(({ label }) => label)
                    .join(", "),
                })}
              </PrimaryText>
            )}
          {duplicateTransactions.size > 0 && (
            <DuplicateTransactionTable
              tableData={duplicateTransactions}
              matchFields={duplicateMatchFields}
              restoreTransaction={restoreImportedTransaction}
            />
          )}
          {!filterDuplicatesField.getValue() && (
            <Alert
              variant="outline"
              color="var(--text-color-status-warn)"
              icon={<InfoIcon />}
            >
              <PrimaryText size="sm">
                {t("duplicate_review_filter_disabled_message")}
              </PrimaryText>
            </Alert>
          )}
          {filterDuplicatesField.getValue() &&
            activeFilterFields.length > 0 &&
            duplicateTransactions.size === 0 && (
              <Alert
                variant="outline"
                color="var(--text-color-status-good)"
                icon={<InfoIcon />}
              >
                <PrimaryText size="sm">
                  {t("duplicate_review_no_duplicates_message")}
                </PrimaryText>
              </Alert>
            )}
          {filteredTransactions.length === 0 && (
            <Alert
              variant="outline"
              color="var(--text-color-status-bad)"
              icon={<InfoIcon />}
            >
              <PrimaryText size="sm">
                {t("duplicate_review_no_transactions_message")}
              </PrimaryText>
            </Alert>
          )}
        </Stack>
      )}
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
          disabled={
            transactionsQuery.isPending || filteredTransactions.length === 0
          }
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
