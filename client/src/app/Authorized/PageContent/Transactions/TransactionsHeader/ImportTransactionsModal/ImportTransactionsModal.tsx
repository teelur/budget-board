import { Button, Stepper } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { FileDownIcon } from "lucide-react";
import React from "react";
import {
  IAccountNameToIDKeyValuePair,
  ITransactionImportDuplicateFieldAvailability,
  ITransactionImportDuplicateOptions,
  ITransactionImportRequest,
  ITransactionImportTableData,
} from "~/models/transaction";
import LoadCsv, { CsvRow } from "./LoadCsv/LoadCsv";
import AccountMapping from "./AccountMapping/AccountMapping";
import ConfigureTransactions from "./ConfigureTransactions/ConfigureTransactions";
import { useDisclosure } from "@mantine/hooks";
import ImportCompleted from "./ImportCompleted/ImportCompleted";
import Modal from "~/components/core/Modal/Modal";
import { useTranslation } from "react-i18next";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useImportTransactionsMutation } from "~/hooks/mutations/transactions/useImportTransactionsMutation";
import ImportProgress from "./ImportProgress/ImportProgress";
import { useTransactionImportJob } from "~/providers/TransactionImportJobProvider/TransactionImportJobProvider";
import DuplicateReview from "./DuplicateReview/DuplicateReview";
import { normalizeImportedAccountName } from "~/helpers/transactionImport";

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);
  const [activeStep, setActiveStep] = React.useState(0);

  const { t } = useTranslation();
  const importTransactionsMutation = useImportTransactionsMutation();
  const {
    activeJobId,
    job,
    isLoading,
    isCancelling,
    startImport,
    cancelImport,
  } = useTransactionImportJob();
  const trackedJobId = React.useRef<string | null>(null);

  // Load CSV Dialog Data
  const [headers, setHeaders] = React.useState<string[]>([]);
  const [csvData, setCsvData] = React.useState<CsvRow[]>([]);

  // Configure Transactions Dialog Data
  const [importData, setImportData] = React.useState<
    ITransactionImportTableData[]
  >([]);
  const [duplicateOptions, setDuplicateOptions] =
    React.useState<ITransactionImportDuplicateOptions>({
      filterDuplicates: true,
      filterByOptions: {
        date: true,
        merchantName: false,
        category: false,
        amount: true,
        account: true,
      },
    });
  const [availableDuplicateFields, setAvailableDuplicateFields] =
    React.useState<ITransactionImportDuplicateFieldAvailability>({
      date: false,
      merchantName: false,
      category: false,
      amount: false,
      account: false,
    });
  const [mappedImportData, setMappedImportData] = React.useState<
    ITransactionImportTableData[]
  >([]);

  // Account Mapping Dialog Data
  const [accountNameToAccountIdMap, setAccountNameToAccountIdMap] =
    React.useState<Map<string, string>>(new Map<string, string>());

  const resetData = () => {
    setHeaders([]);
    setCsvData([]);

    setImportData([]);
    setMappedImportData([]);
    setDuplicateOptions({
      filterDuplicates: true,
      filterByOptions: {
        date: true,
        merchantName: false,
        category: false,
        amount: true,
        account: true,
      },
    });
    setAvailableDuplicateFields({
      date: false,
      merchantName: false,
      category: false,
      amount: false,
      account: false,
    });

    setAccountNameToAccountIdMap(new Map<string, string>());
  };

  React.useEffect(() => {
    if (
      trackedJobId.current === job?.id &&
      ["Completed", "CompletedWithErrors", "Failed", "Cancelled"].includes(
        job.status,
      )
    ) {
      setActiveStep(5);
    }
  }, [job]);

  /**
   * Handle CSV import data from the file loader component.
   *
   * Parameters:
   * @param headers - Array of column names from the CSV header row.
   * @param rows - Array of parsed CSV rows returned by the CSV loader. Each
   *               row is a `CsvRow` object produced by `LoadCsv`.
   */
  const importCsvData = (headers: string[], rows: CsvRow[]) => {
    try {
      if (headers.length > 0) {
        setHeaders(headers);
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("csv_file_missing_header_row_message"),
        });
        return;
      }

      setCsvData(rows);
    } catch (error) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("error_reading_file_message", { error }),
      });
      resetData();
    }
  };

  React.useEffect(() => {
    // Whenever the import data changes, rebuild the account name to ID map
    const newMap = new Map<string, string>();
    importData.forEach((transaction) => {
      const accountName = transaction.account?.trim();
      if (accountName && !newMap.has(accountName)) {
        newMap.set(accountName, "");
      }
    });
    setAccountNameToAccountIdMap(newMap);
  }, [importData]);

  const onSubmit = async (
    filteredImportedData: ITransactionImportTableData[],
  ) => {
    if (filteredImportedData.length === 0) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("no_transactions_to_import_message"),
      });
      return;
    }

    const accountNameToAccountArray: IAccountNameToIDKeyValuePair[] =
      Array.from(accountNameToAccountIdMap.entries())
        .filter(([_, accountID]) => !!accountID)
        .map(
          ([accountName, accountID]) =>
            ({
              accountName,
              accountID,
            }) as IAccountNameToIDKeyValuePair,
        );

    const transactionImportRequest: ITransactionImportRequest = {
      transactions: filteredImportedData.map((transaction) => ({
        ...transaction,
        account: normalizeImportedAccountName(transaction.account) || null,
      })),
      accountNameToIDMap: accountNameToAccountArray,
    };

    importTransactionsMutation.mutate(
      {
        importedTransactions: transactionImportRequest,
        idempotencyKey: crypto.randomUUID(),
      },
      {
        onSuccess: (response) => {
          trackedJobId.current = response.data.id;
          startImport(response.data.id);
          setActiveStep(4);
        },
      },
    );
  };

  const advanceToAccountMappingDialog = (
    importData: ITransactionImportTableData[],
    availableFields: ITransactionImportDuplicateFieldAvailability,
  ) => {
    setImportData(importData);
    setAvailableDuplicateFields(availableFields);
    setActiveStep(2);
  };

  const advanceToDuplicateReviewDialog = (
    mappedData: ITransactionImportTableData[],
    accountMap: Map<string, string>,
  ) => {
    setMappedImportData(mappedData);
    setAccountNameToAccountIdMap(accountMap);
    setActiveStep(3);
  };

  return (
    <>
      <Button
        size="sm"
        rightSection={<FileDownIcon size="1rem" />}
        onClick={() => {
          resetData();
          if (activeJobId && job?.id === activeJobId) {
            trackedJobId.current = activeJobId;
            setActiveStep(4);
          } else {
            trackedJobId.current = null;
            setActiveStep(0);
          }
          open();
        }}
      >
        {t("import")}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        size="auto"
        title={
          <PrimaryHeading component="span" order={4}>
            {t("import_transactions")}
          </PrimaryHeading>
        }
      >
        <Stepper
          active={activeStep}
          allowNextStepsSelect={false}
          w="100%"
          mb="1rem"
        >
          <Stepper.Step label={t("step_1")} description={t("load_csv")}>
            <LoadCsv
              loadCsv={importCsvData}
              launchNextDialog={() => setActiveStep(1)}
            />
          </Stepper.Step>
          <Stepper.Step
            label={t("step_2")}
            description={t("configure_transactions")}
          >
            <ConfigureTransactions
              csvData={csvData}
              csvHeaders={headers}
              advanceToNextDialog={advanceToAccountMappingDialog}
              goBackToPreviousDialog={() => setActiveStep(0)}
            />
          </Stepper.Step>
          <Stepper.Step label={t("step_3")} description={t("map_accounts")}>
            <AccountMapping
              importedTransactions={importData}
              accountNameToAccountIdMap={accountNameToAccountIdMap}
              setAccountNameToAccountIdMap={setAccountNameToAccountIdMap}
              goBackToPreviousDialog={() => setActiveStep(1)}
              advanceToNextDialog={advanceToDuplicateReviewDialog}
            />
          </Stepper.Step>
          <Stepper.Step
            label={t("step_4")}
            description={t("duplicate_transactions")}
          >
            <DuplicateReview
              importedTransactions={mappedImportData}
              duplicateOptions={duplicateOptions}
              setDuplicateOptions={setDuplicateOptions}
              availableDuplicateFields={availableDuplicateFields}
              accountNameToAccountIdMap={accountNameToAccountIdMap}
              goBackToPreviousDialog={() => setActiveStep(2)}
              advanceToNextDialog={onSubmit}
            />
          </Stepper.Step>
          <Stepper.Step
            label={t("step_5")}
            description={t("import_progress_step")}
          >
            <ImportProgress
              job={job}
              isLoading={isLoading}
              isCancelling={isCancelling}
              onCancel={async () => {
                await cancelImport();
                close();
              }}
            />
          </Stepper.Step>
          <Stepper.Completed>
            <ImportCompleted
              goBackToPreviousDialog={() => setActiveStep(3)}
              closeModal={close}
              hasErrors={job?.status === "CompletedWithErrors"}
              isCancelled={job?.status === "Cancelled"}
              isFailed={job?.status === "Failed"}
            />
          </Stepper.Completed>
        </Stepper>
      </Modal>
    </>
  );
};

export default ImportTransactionsModal;
