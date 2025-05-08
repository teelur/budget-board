import { Button, Modal, Stack } from "@mantine/core";
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
} from "~/models/transaction";
import CsvOptions from "./CsvOptions/CsvOptions";
import TransactionsTable from "./TransactionsTable/TransactionsTable";
import ColumnsSelect from "./ColumnsSelect/ColumnsSelect";
import ColumnsOptions from "./ColumnsOptions/ColumnsOptions";
import AccountMapping from "./AccountMapping/AccountMapping";

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);
  const [headers, setHeaders] = React.useState<string[]>([]);
  const [csvData, setCsvData] = React.useState<unknown[]>([]);
  const [importedData, setImportedData] = React.useState<ITransactionImport[]>(
    []
  );
  const [accountNameToAccountIdMap, setAccountNameToAccountIdMap] =
    React.useState<Map<string, string>>(new Map<string, string>());

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

  const setColumn = (column: string, value: string) => {
    switch (column) {
      case "date":
        dateField.setValue(value);
        break;
      case "description":
        descriptionField.setValue(value);
        break;
      case "category":
        categoryField.setValue(value);
        break;
      case "amount":
        amountField.setValue(value);
        break;
      case "account":
        accountField.setValue(value);
        break;
    }
    handleColumnsChange();
  };

  const resetData = () => {
    fileField.reset();
    delimiterField.reset();

    setHeaders([]);
    setCsvData([]);
    setImportedData([]);

    dateField.reset();
    descriptionField.reset();
    categoryField.reset();
    amountField.reset();
    accountField.reset();

    includeExpensesColumnField.reset();
    invertAmountField.reset();

    setAccountNameToAccountIdMap(new Map<string, string>());
  };

  const handleColumnsChange = () => {
    if (
      !dateField.getValue() &&
      !descriptionField.getValue() &&
      !categoryField.getValue() &&
      !amountField.getValue() &&
      !accountField.getValue()
    ) {
      setImportedData([]);
      setAccountNameToAccountIdMap(new Map<string, string>());
      return;
    }

    const importedTransactions: ITransactionImport[] = csvData.map(
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
        account: accountField.getValue() ? row[accountField.getValue()!] : null,
      })
    );

    setImportedData(importedTransactions);

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
  };

  // TODO: Clean this up.
  const handleFileChange = async () => {
    if (fileField.getValue() == null) {
      resetData();
      return;
    }
    if (fileField.getValue()!.type !== "text/csv") {
      resetData();
      return;
    }

    const text = await fileField.getValue()!.text();

    const parsed = Papa.parse(text, {
      header: true,
      skipEmptyLines: true,
      dynamicTyping: true,
      delimiter: delimiterField.getValue(),
    });

    if (parsed.errors.length > 0) {
      console.error("Error parsing CSV:", parsed.errors);
      return;
    }
    if (parsed.data.length === 0) {
      console.error("No data found in CSV");
      return;
    }
    if (parsed.meta.fields) {
      setHeaders(parsed.meta.fields);
    } else {
      console.error("No headers found in CSV");
      return;
    }

    setCsvData(parsed.data);
    setImportedData([]);

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
          amount: amountField.getValue() ? row[amountField.getValue()!] : null,
          account: accountField.getValue()
            ? row[accountField.getValue()!]
            : null,
        })
      );

      setImportedData(importedTransactions);

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

  const onSubmit = async () => {
    const transactionImportRequest: ITransactionImportRequest = {
      transactions: importedData.map((transaction) => ({
        date: transaction.date,
        description: transaction.description,
        category: transaction.category,
        amount: transaction.amount,
        account: transaction.account,
      })),
      accountNameToIDMap: Array.from(accountNameToAccountIdMap.entries()).map(
        ([accountName, accountID]) => ({
          accountName,
          accountID,
        })
      ),
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
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <Stack>
          <CsvOptions
            fileField={fileField.getValue()}
            setFileField={fileField.setValue}
            delimiterField={delimiterField.getValue()}
            setDelimiterField={delimiterField.setValue}
            handleFileChange={handleFileChange}
          />
          {importedData.length > 0 && (
            <TransactionsTable
              tableData={importedData}
              setTableData={setImportedData}
              setCsvData={setCsvData}
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
            />
          )}
          {accountNameToAccountIdMap.size > 0 && (
            <AccountMapping
              accountNameToAccountIdMap={accountNameToAccountIdMap}
              setAccountNameToAccountIdMap={setAccountNameToAccountIdMap}
            />
          )}
          <Button onClick={onSubmit} loading={doImportMutation.isPending}>
            Import
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default ImportTransactionsModal;
