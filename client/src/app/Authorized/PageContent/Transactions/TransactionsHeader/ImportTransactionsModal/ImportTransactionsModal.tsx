import { Button, LoadingOverlay, Modal, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ImportIcon } from "lucide-react";
import Papa from "papaparse";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import {
  ITransactionImport,
  ITransactionImportRequest,
  ITransactionImportTableData,
  TransactionImportTableData,
} from "~/models/transaction";
import CsvOptions from "./CsvOptions/CsvOptions";
import TransactionsTable from "./TransactionsTable/TransactionsTable";
import ColumnsSelect from "./ColumnsSelect/ColumnsSelect";
import ColumnsOptions from "./ColumnsOptions/ColumnsOptions";
import AccountMapping from "./AccountMapping/AccountMapping";

// TODO: There is probably some optimization that can be done here.

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);

  const [isLoading, setIsLoading] = React.useState(false);
  const [headers, setHeaders] = React.useState<string[]>([]);
  const [csvData, setCsvData] = React.useState<unknown[]>([]);
  const [importedData, setImportedData] = React.useState<ITransactionImport[]>(
    []
  );
  const [importedTransactionsTableData, setImportedTransactionsTableData] =
    React.useState<ITransactionImportTableData[]>([]);
  const [accountNameToAccountIdMap, setAccountNameToAccountIdMap] =
    React.useState<Map<string, string>>(new Map<string, string>());

  const fileField = useField<File | null>({
    initialValue: null,
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return;
      }
      if (value.type !== "text/csv") {
        return "File must be a CSV file";
      }
      return null;
    },
  });
  const delimiterField = useField<string>({
    initialValue: ",",
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return "Delimiter is required";
      }
      if (value.length > 1) {
        return "Delimiter must be a single character";
      }
      return null;
    },
  });
  const dateField = useField<string | null>({
    initialValue: null,
  });
  const descriptionField = useField<string | null>({
    initialValue: null,
  });
  const categoryField = useField<string | null>({
    initialValue: null,
  });
  const amountField = useField<string | null>({
    initialValue: null,
  });
  const accountField = useField<string | null>({
    initialValue: null,
  });
  const includeExpensesColumnField = useField<boolean>({
    initialValue: false,
  });
  const invertAmountField = useField<boolean>({
    initialValue: false,
  });
  const expensesColumnField = useField<string | null>({
    initialValue: "",
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return "Expenses column name is required";
      }
      return null;
    },
  });
  const expensesColumnValueField = useField<string | null>({
    initialValue: null,
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return "Expenses column value is required";
      }
      return null;
    },
  });

  const expensesColumnValues: string[] = React.useMemo(() => {
    const columnValues = csvData.map((row: any) => {
      if (!expensesColumnField.getValue()) {
        return null;
      }

      const value = row[expensesColumnField.getValue()!];
      if (value) {
        return value;
      }
      return null;
    });
    const uniqueValues = Array.from(new Set(columnValues)).filter(
      (value) => value !== null
    );
    return uniqueValues.map((value) => value.toString());
  }, [
    csvData,
    expensesColumnField.getValue(),
    expensesColumnValueField.getValue(),
  ]);

  const resetData = () => {
    fileField.reset();

    setHeaders([]);
    setCsvData([]);
    setImportedData([]);
    setImportedTransactionsTableData([]);

    dateField.reset();
    descriptionField.reset();
    categoryField.reset();
    amountField.reset();
    accountField.reset();

    invertAmountField.reset();
    includeExpensesColumnField.reset();
    expensesColumnField.reset();
    expensesColumnValueField.reset();

    setAccountNameToAccountIdMap(new Map<string, string>());
  };

  const processFile = async () => {
    try {
      setIsLoading(true);
      const file = fileField.getValue();
      const delimiter = delimiterField.getValue();

      // We don't want to parse the file if the user hasn't defined the file or delimiter.
      if (
        !file ||
        file.type !== "text/csv" ||
        !delimiter ||
        delimiter.length !== 1
      ) {
        return;
      }

      const text = await file.text();
      const parsed = Papa.parse(text, {
        header: true,
        skipEmptyLines: true,
        dynamicTyping: true,
        delimiter,
      });

      // Display any errors that occurred during parsing.
      if (parsed.errors.length > 0) {
        const uniqueErrorMessages = Array.from(
          new Set(parsed.errors.map((error) => error.message))
        );

        uniqueErrorMessages.forEach((errorMessage) => {
          notifications.show({
            color: "red",
            message: `Error parsing CSV: ${errorMessage}`,
          });
        });

        resetData();
        return;
      }

      if (parsed.data.length === 0) {
        notifications.show({
          color: "red",
          message: "CSV file is empty",
        });
        resetData();
        return;
      }

      if (parsed.meta.fields) {
        setHeaders(parsed.meta.fields);
      } else {
        notifications.show({
          color: "red",
          message: "CSV file has no headers",
        });
        resetData();
        return;
      }

      setCsvData(parsed.data);

      // The headers will auto-populate if they match the default values
      dateField.setValue(
        parsed.meta.fields?.find((header) =>
          areStringsEqual(header.toLowerCase(), "date")
        ) ?? null
      );
      descriptionField.setValue(
        parsed.meta.fields?.find((header) =>
          areStringsEqual(header.toLowerCase(), "description")
        ) ?? null
      );
      categoryField.setValue(
        parsed.meta.fields?.find((header) =>
          areStringsEqual(header.toLowerCase(), "category")
        ) ?? null
      );
      amountField.setValue(
        parsed.meta.fields?.find((header) =>
          areStringsEqual(header.toLowerCase(), "amount")
        ) ?? null
      );
      accountField.setValue(
        parsed.meta.fields?.find((header) =>
          areStringsEqual(header.toLowerCase(), "account")
        ) ?? null
      );

      // The table will auto-populate if any of the headers are defined.
      if (
        dateField.getValue() ||
        descriptionField.getValue() ||
        categoryField.getValue() ||
        amountField.getValue() ||
        accountField.getValue()
      ) {
        const importedTransactions: ITransactionImport[] = parsed.data.map(
          (row: any) => ({
            date: dateField.getValue()
              ? new Date(row[dateField.getValue()!])
              : null,
            description: descriptionField.getValue()
              ? row[descriptionField.getValue()!]
              : null,
            category: categoryField.getValue()
              ? row[categoryField.getValue()!]
              : null,
            amount: amountField.getValue()
              ? row[amountField.getValue()!]
              : null,
            account: accountField.getValue()
              ? row[accountField.getValue()!]
              : null,
            type: null,
          })
        );

        setImportedData(importedTransactions);
        setImportedTransactionsTableData(
          importedTransactions.map(
            (i) => new TransactionImportTableData(i, null)
          )
        );

        // The user will need to map the imported account names to existing accounts in the app.
        if (accountField.getValue()) {
          const accountNameToAccountIdMap = new Map<string, string>();
          importedTransactions.forEach((transaction) => {
            if (
              transaction.account &&
              !accountNameToAccountIdMap.has(transaction.account)
            ) {
              accountNameToAccountIdMap.set(transaction.account, "");
            }
          });
          setAccountNameToAccountIdMap(accountNameToAccountIdMap);
        }
      } else {
        setImportedData([]);
        setImportedTransactionsTableData([]);
        setAccountNameToAccountIdMap(new Map<string, string>());
      }
    } finally {
      setIsLoading(false);
    }
  };

  const parseFileData = (doParseAccounts?: boolean) => {
    try {
      setIsLoading(true);
      // We don't want the table to show a bunch of empty rows if none of the columns are set.
      if (
        !dateField.getValue() &&
        !descriptionField.getValue() &&
        !categoryField.getValue() &&
        !amountField.getValue() &&
        !accountField.getValue()
      ) {
        setImportedData([]);
        setImportedTransactionsTableData([]);
        setAccountNameToAccountIdMap(new Map<string, string>());
        return;
      }

      const includeExpensesColumn = includeExpensesColumnField.getValue();
      const expensesColumnName = expensesColumnField.getValue();

      const importedTransactions: ITransactionImportTableData[] = csvData.map(
        (row: any) => ({
          date: dateField.getValue()
            ? new Date(row[dateField.getValue()!])
            : null,
          description: descriptionField.getValue()
            ? row[descriptionField.getValue()!]
            : null,
          category: categoryField.getValue()
            ? row[categoryField.getValue()!]
            : null,
          amount: amountField.getValue() ? row[amountField.getValue()!] : null,
          account: accountField.getValue()
            ? row[accountField.getValue()!]
            : null,
          type:
            includeExpensesColumn &&
            expensesColumnName &&
            expensesColumnName.length > 0
              ? row[expensesColumnName]
              : null,
        })
      );

      const expensesColumnValue = expensesColumnValueField.getValue();

      if (includeExpensesColumn && expensesColumnName && expensesColumnValue) {
        importedTransactions.forEach((transaction) => {
          if (areStringsEqual(transaction.type ?? "", expensesColumnValue)) {
            transaction.amount = transaction.amount
              ? transaction.amount * -1
              : null;
          }
        });
      }

      if (invertAmountField.getValue()) {
        importedTransactions.forEach((transaction) => {
          if (transaction.amount) {
            transaction.amount = transaction.amount
              ? transaction.amount * -1
              : null;
          }
        });
      }

      setImportedData(importedTransactions);
      setImportedTransactionsTableData(importedTransactions);

      if (doParseAccounts) {
        const accountNameToAccountIdMap = new Map<string, string>();
        importedTransactions.forEach((transaction) => {
          if (
            transaction.account &&
            !accountNameToAccountIdMap.has(transaction.account)
          ) {
            accountNameToAccountIdMap.set(transaction.account, "");
          }
        });
        setAccountNameToAccountIdMap(accountNameToAccountIdMap);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const setColumn = (column: string, value: string) => {
    switch (column) {
      case "date":
        dateField.setValue(value);
        parseFileData();
        break;
      case "description":
        descriptionField.setValue(value);
        parseFileData();
        break;
      case "category":
        categoryField.setValue(value);
        parseFileData();
        break;
      case "amount":
        amountField.setValue(value);
        parseFileData();
        break;
      case "account":
        accountField.setValue(value);
        parseFileData(true);
        break;
    }
  };

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
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  const filteredImportedData = React.useMemo(() => {
    return importedData.filter(
      (t) =>
        !areStringsEqual(
          accountNameToAccountIdMap.get(t.account ?? "") ?? "",
          "exclude"
        ) &&
        !areStringsEqual(
          accountNameToAccountIdMap.get(t.account ?? "") ?? "",
          ""
        )
    );
  }, [importedData, accountNameToAccountIdMap]);

  const onSubmit = async () => {
    if (filteredImportedData.length === 0) {
      notifications.show({
        color: "red",
        message: "No transactions to import",
      });
      return;
    }

    const accountNameToAccountArray = Array.from(
      accountNameToAccountIdMap.entries()
    )
      .filter(([_, accountID]) => !!accountID)
      .map(([accountName, accountID]) => ({
        accountName,
        accountID,
      }));

    const transactionImportRequest: ITransactionImportRequest = {
      transactions: filteredImportedData,
      accountNameToIDMap: accountNameToAccountArray,
    };

    doImportMutation.mutate(transactionImportRequest);
  };

  return (
    <>
      <Button
        size="sm"
        rightSection={<ImportIcon size="1rem" />}
        onClick={open}
      >
        Import
      </Button>
      <Modal
        opened={opened}
        onClose={() => {
          close();
          resetData();
        }}
        title="Import Transactions"
        size="auto"
        p="0.5rem"
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <LoadingOverlay visible={isLoading} />
        <Stack>
          <CsvOptions
            fileField={fileField.getValue()}
            setFileField={fileField.setValue}
            delimiterField={delimiterField.getValue()}
            setDelimiterField={delimiterField.setValue}
            handleFileChange={processFile}
            resetData={resetData}
          />
          {importedTransactionsTableData.length > 0 && (
            <TransactionsTable
              tableData={importedTransactionsTableData}
              setTableData={setImportedTransactionsTableData}
              setCsvData={setCsvData}
              setImportedData={setImportedData}
            />
          )}
          {headers.length > 0 && (
            <ColumnsSelect
              columns={headers}
              date={dateField.getValue()}
              description={descriptionField.getValue()}
              category={categoryField.getValue()}
              amount={amountField.getValue()}
              account={accountField.getValue()}
              setColumn={setColumn}
            />
          )}
          {headers.length > 0 && (
            <ColumnsOptions
              invertAmount={invertAmountField.getValue()}
              setInvertAmount={invertAmountField.setValue}
              includeExpensesColumn={includeExpensesColumnField.getValue()}
              setIncludeExpensesColumn={includeExpensesColumnField.setValue}
              columns={headers}
              expensesColumn={expensesColumnField.getValue()}
              setExpensesColumn={expensesColumnField.setValue}
              expensesColumnValues={expensesColumnValues}
              expensesColumnValue={expensesColumnValueField.getValue()}
              setExpensesColumnValue={expensesColumnValueField.setValue}
              handleAmountChange={parseFileData}
            />
          )}
          {accountNameToAccountIdMap.size > 0 && (
            <AccountMapping
              accountNameToAccountIdMap={accountNameToAccountIdMap}
              setAccountNameToAccountIdMap={setAccountNameToAccountIdMap}
            />
          )}
          <Button
            onClick={onSubmit}
            loading={doImportMutation.isPending}
            disabled={
              doImportMutation.isPending || filteredImportedData.length === 0
            }
          >
            Import {filteredImportedData.length} Transactions
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default ImportTransactionsModal;
