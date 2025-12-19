import { Button, Stepper } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ImportIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IAccountNameToIDKeyValuePair,
  ITransactionImport,
  ITransactionImportRequest,
  ITransactionImportTableData,
} from "~/models/transaction";
import LoadCsv, { CsvRow } from "./LoadCsv/LoadCsv";
import AccountMapping from "./AccountMapping/AccountMapping";
import ConfigureTransactions from "./ConfigureTransactions/ConfigureTransactions";
import { useDisclosure } from "@mantine/hooks";
import ImportCompleted from "./ImportCompleted/ImportCompleted";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);
  const [activeStep, setActiveStep] = React.useState(0);

  const { t } = useTranslation();

  // Load CSV Dialog Data
  const [headers, setHeaders] = React.useState<string[]>([]);
  const [csvData, setCsvData] = React.useState<CsvRow[]>([]);

  // Configure Transactions Dialog Data
  const [importData, setImportData] = React.useState<
    ITransactionImportTableData[]
  >([]);

  // Account Mapping Dialog Data
  const [accountNameToAccountIdMap, setAccountNameToAccountIdMap] =
    React.useState<Map<string, string>>(new Map<string, string>());

  const resetData = () => {
    setHeaders([]);
    setCsvData([]);

    setImportData([]);

    setAccountNameToAccountIdMap(new Map<string, string>());
  };

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

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doImportMutation = useMutation({
    mutationFn: async (importedTransactions: ITransactionImportRequest) =>
      await request({
        url: "/api/transaction/import",
        method: "POST",
        data: importedTransactions,
      }),
    onSuccess: async () => {
      setActiveStep(3);
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const onSubmit = async (
    filteredImportedData: ITransactionImportTableData[]
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
            } as IAccountNameToIDKeyValuePair)
        );

    const transactionImportRequest: ITransactionImportRequest = {
      transactions: filteredImportedData as ITransactionImport[],
      accountNameToIDMap: accountNameToAccountArray,
    };

    doImportMutation.mutate(transactionImportRequest);
  };

  const advanceToAccountMappingDialog = (
    importData: ITransactionImportTableData[]
  ) => {
    setImportData(importData);
    setActiveStep(2);
  };

  return (
    <>
      <Button
        size="sm"
        rightSection={<ImportIcon size="1rem" />}
        onClick={() => {
          resetData();
          setActiveStep(0);
          open();
        }}
      >
        {t("import_transactions")}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        size="auto"
        title={<PrimaryText>{t("import_transactions")}</PrimaryText>}
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
              submitImport={onSubmit}
              isSubmitting={doImportMutation.isPending}
            />
          </Stepper.Step>
          <Stepper.Completed>
            <ImportCompleted
              goBackToPreviousDialog={() => setActiveStep(2)}
              closeModal={close}
            />
          </Stepper.Completed>
        </Stepper>
      </Modal>
    </>
  );
};

export default ImportTransactionsModal;
