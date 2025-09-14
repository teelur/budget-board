import { Button, Modal, Text, useModalsStack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ImportIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
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

const ImportTransactionsModal = () => {
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
      notifications.show({
        color: "green",
        message: "Transactions imported successfully",
      });
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

  const stack = useModalsStack([
    "load-csv",
    "configure-import",
    "account-mapping",
  ]);

  const advanceToAccountMappingDialog = (
    importData: ITransactionImportTableData[]
  ) => {
    setImportData(importData);
    stack.open("account-mapping");
  };

  return (
    <>
      <Button
        size="sm"
        rightSection={<ImportIcon size="1rem" />}
        onClick={() => stack.open("load-csv")}
      >
        Import
      </Button>
      <Modal.Stack>
        <Modal
          {...stack.register("load-csv")}
          size="auto"
          p="0.5rem"
          title={<Text fw={600}>Load CSV</Text>}
          onClose={() => stack.closeAll()}
        >
          <LoadCsv
            loadCsv={importCsvData}
            launchNextDialog={() => stack.open("configure-import")}
          />
        </Modal>
        <Modal
          {...stack.register("configure-import")}
          size="auto"
          p="0.5rem"
          title={<Text fw={600}>Configure Transactions</Text>}
          onClose={() => stack.closeAll()}
        >
          <ConfigureTransactions
            csvData={csvData}
            csvHeaders={headers}
            advanceToNextDialog={advanceToAccountMappingDialog}
            goBackToPreviousDialog={() => stack.close("configure-import")}
          />
        </Modal>
        <Modal
          {...stack.register("account-mapping")}
          size="auto"
          p="0.5rem"
          title={<Text fw={600}>Account Mapping</Text>}
          onClose={() => stack.closeAll()}
        >
          <AccountMapping
            importedTransactions={importData}
            accountNameToAccountIdMap={accountNameToAccountIdMap}
            setAccountNameToAccountIdMap={setAccountNameToAccountIdMap}
            goBackToPreviousDialog={() => stack.close("account-mapping")}
            submitImport={onSubmit}
            isSubmitting={doImportMutation.isPending}
          />
        </Modal>
      </Modal.Stack>
    </>
  );
};

export default ImportTransactionsModal;
