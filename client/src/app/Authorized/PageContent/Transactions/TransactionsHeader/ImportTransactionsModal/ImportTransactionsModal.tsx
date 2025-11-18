import { Button, Modal, Stepper, Text } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ImportIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
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

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);
  const [activeStep, setActiveStep] = React.useState(0);

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
          color: "red",
          message: "CSV file is missing a header row",
        });
        return;
      }

      setCsvData(rows);
    } catch (error) {
      notifications.show({
        color: "red",
        message: `Error reading file: ${error}`,
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

  const { request } = React.useContext<any>(AuthContext);

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
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  const onSubmit = async (
    filteredImportedData: ITransactionImportTableData[]
  ) => {
    if (filteredImportedData.length === 0) {
      notifications.show({
        color: "red",
        message: "No transactions to import",
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
        Import
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        size="auto"
        p="0.5rem"
        title={<Text fw={600}>Import Transactions</Text>}
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <Stepper
          active={activeStep}
          allowNextStepsSelect={false}
          w="100%"
          mb="1rem"
        >
          <Stepper.Step label="Step 1" description="Load CSV">
            <LoadCsv
              loadCsv={importCsvData}
              launchNextDialog={() => setActiveStep(1)}
            />
          </Stepper.Step>
          <Stepper.Step label="Step 2" description="Configure Transactions">
            <ConfigureTransactions
              csvData={csvData}
              csvHeaders={headers}
              advanceToNextDialog={advanceToAccountMappingDialog}
              goBackToPreviousDialog={() => setActiveStep(0)}
            />
          </Stepper.Step>
          <Stepper.Step label="Step 3" description="Map Accounts">
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
