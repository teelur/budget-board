import { Button, LoadingOverlay, Modal, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { ImportIcon } from "lucide-react";
import Papa from "papaparse";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import {
  defaultTransactionCategories,
  IAccountNameToIDKeyValuePair,
  ITransaction,
  ITransactionImport,
  ITransactionImportRequest,
  ITransactionImportTableData,
} from "~/models/transaction";
import CsvOptions from "./CsvOptions/CsvOptions";
import TransactionsTable from "./TransactionsTable/TransactionsTable";
import ColumnsSelect from "./ColumnsSelect/ColumnsSelect";
import ColumnsOptions from "./ColumnsOptions/ColumnsOptions";
import AccountMapping from "./AccountMapping/AccountMapping";
import DuplicateTransactionTable from "./DuplicateTransactionTable/DuplicateTransactionTable";
import { areDatesEqual } from "~/helpers/datetime";
import { IAccount } from "~/models/account";
import { ICategoryResponse } from "~/models/category";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import dayjs from "dayjs";

const ImportTransactionsModal = () => {
  const [opened, { open, close }] = useDisclosure(false);

  const [isLoading, setIsLoading] = React.useState(false);
  const [headers, setHeaders] = React.useState<string[]>([]);
  const [csvData, setCsvData] = React.useState<unknown[]>([]);
  const [importedTransactionsTableData, setImportedTransactionsTableData] =
    React.useState<ITransactionImportTableData[]>([]);
  const [accountNameToAccountIdMap, setAccountNameToAccountIdMap] =
    React.useState<Map<string, string>>(new Map<string, string>());
  const [duplicateTransactions, setDuplicateTransactions] = React.useState<
    Map<ITransactionImportTableData, ITransaction>
  >(new Map<ITransactionImportTableData, ITransaction>());

  // Columns Fields
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
  const incomeField = useField<string | null>({
    initialValue: null,
  });
  const expenseField = useField<string | null>({
    initialValue: null,
  });
  const accountField = useField<string | null>({
    initialValue: null,
  });

  // Columns Options
  const dateFormatField = useField<string | null>({
    initialValue: "MM/DD/YYYY",
  });
  const invertAmountField = useField<boolean>({
    initialValue: false,
  });
  const splitIntoSeparateColumnsField = useField<boolean>({
    initialValue: false,
  });
  const includeExpensesColumnField = useField<boolean>({
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

  const filterDuplicatesField = useField<boolean>({
    initialValue: false,
  });
  const filterByDateField = useField<boolean>({
    initialValue: false,
  });
  const filterByDescriptionField = useField<boolean>({
    initialValue: false,
  });
  const filterByCategoryField = useField<boolean>({
    initialValue: false,
  });
  const filterByAmountField = useField<boolean>({
    initialValue: false,
  });
  const filterByAccountField = useField<boolean>({
    initialValue: false,
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

  const { request } = React.useContext<any>(AuthContext);
  const transactionsQuery = useQuery({
    queryKey: ["transactions", { getHidden: false }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ITransaction[];
      }

      return [];
    },
  });

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccount[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccount[];
      }

      return [];
    },
  });

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

  const resetColumnsOptions = () => {
    dateFormatField.reset();
    invertAmountField.reset();
    splitIntoSeparateColumnsField.reset();
    includeExpensesColumnField.reset();
    expensesColumnField.reset();
    expensesColumnValueField.reset();
    filterDuplicatesField.reset();

    setDuplicateTransactions(
      new Map<ITransactionImportTableData, ITransaction>()
    );
  };

  const resetData = () => {
    setHeaders([]);
    setCsvData([]);
    setImportedTransactionsTableData([]);

    dateField.reset();
    descriptionField.reset();
    categoryField.reset();
    amountField.reset();
    incomeField.reset();
    expenseField.reset();
    accountField.reset();

    resetColumnsOptions();

    setAccountNameToAccountIdMap(new Map<string, string>());
  };

  const importCsvFile = async (file: File, delimiter: string) => {
    try {
      setIsLoading(true);

      // We don't want to parse the file if the user hasn't defined the file or delimiter.
      if (
        !file ||
        file.type !== "text/csv" ||
        !delimiter ||
        delimiter.length !== 1
      ) {
        notifications.show({
          color: "red",
          message: "Please provide a valid CSV file and delimiter",
        });
        resetData();
        return;
      }

      const text = await file.text();
      const parsed = Papa.parse(text, {
        header: true,
        skipEmptyLines: true,
        dynamicTyping: true,
        delimitersToGuess: [
          ",",
          "\t",
          "|",
          ";",
          Papa.RECORD_SEP,
          Papa.UNIT_SEP,
          delimiter,
        ],
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

      setCsvData(
        parsed.data.map((row: any, idx: number) => ({ ...row, uid: idx }))
      );
      resetColumnsOptions();

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
    } finally {
      setIsLoading(false);
    }
  };

  const buildTableData = () => {
    try {
      setIsLoading(true);
      if (
        !dateField.getValue() &&
        !descriptionField.getValue() &&
        !categoryField.getValue() &&
        !amountField.getValue() &&
        !incomeField.getValue() &&
        !expenseField.getValue() &&
        !accountField.getValue()
      ) {
        setImportedTransactionsTableData([]);
        setAccountNameToAccountIdMap(new Map<string, string>());
        setDuplicateTransactions(
          new Map<ITransactionImportTableData, ITransaction>()
        );
        return;
      }

      if (!includeExpensesColumnField.getValue()) {
        expensesColumnField.reset();
        expensesColumnValueField.reset();
      }

      const includeExpensesColumn = includeExpensesColumnField.getValue();
      const expensesColumnName = expensesColumnField.getValue();

      const importedTransactions: ITransactionImportTableData[] = csvData.map(
        (row: any) => {
          let amount: number | null = null;
          if (splitIntoSeparateColumnsField.getValue()) {
            const incomeValue: string | null = row[incomeField.getValue()!];
            const expenseValue: string | null = row[expenseField.getValue()!];

            if (incomeValue) {
              amount = parseFloat(incomeValue);
            }
            if (expenseValue) {
              amount = parseFloat(expenseValue) * -1;
            }
          } else {
            const amountValue: string | null = row[amountField.getValue()!];
            if (amountValue) {
              amount = parseFloat(amountValue);
            }
          }

          return {
            uid: row.uid,
            date: dateField.getValue()
              ? dayjs(
                  row[dateField.getValue()!],
                  dateFormatField.getValue() ?? "MM/DD/YYYY"
                ).toDate()
              : null,
            description: descriptionField.getValue()
              ? row[descriptionField.getValue()!]
              : null,
            category: categoryField.getValue()
              ? row[categoryField.getValue()!]
              : null,
            amount,
            account: accountField.getValue()
              ? row[accountField.getValue()!]
              : null,
            type:
              includeExpensesColumn &&
              expensesColumnName &&
              expensesColumnName.length > 0
                ? row[expensesColumnName]
                : null,
          } as ITransactionImportTableData;
        }
      );

      if (invertAmountField.getValue()) {
        importedTransactions.forEach((transaction) => {
          if (transaction.amount) {
            transaction.amount *= -1;
          }
        });
      }

      const expensesColumnValue = expensesColumnValueField.getValue();

      if (includeExpensesColumn && expensesColumnName && expensesColumnValue) {
        importedTransactions.forEach((transaction) => {
          if (
            transaction.type &&
            transaction.type.toString() === expensesColumnValue &&
            transaction.amount
          ) {
            transaction.amount *= -1;
          }
        });
      }

      let filteredImportedTransactions;
      if (filterDuplicatesField.getValue()) {
        const sortedTableData = importedTransactions.toSorted(
          (a, b) =>
            new Date(a.date ?? 0).getTime() - new Date(b.date ?? 0).getTime()
        );

        // Only filter if there are transactions to compare
        let transactionsInDateRange: ITransaction[] = [];
        if (sortedTableData.length > 0 && transactionsQuery.data?.length) {
          const tableStartDate = new Date(sortedTableData[0]?.date ?? 0);
          tableStartDate.setHours(0, 0, 0, 0);

          const tableEndDate = new Date(
            sortedTableData[sortedTableData.length - 1]?.date ?? 0
          );
          tableEndDate.setHours(23, 59, 59, 999);

          transactionsInDateRange = transactionsQuery.data.filter(
            (transaction) => {
              const transactionDate = new Date(transaction.date);
              return (
                transactionDate >= tableStartDate &&
                transactionDate <= tableEndDate
              );
            }
          );
        }

        const tempDuplicateTransactions: Map<
          ITransactionImportTableData,
          ITransaction
        > = new Map<ITransactionImportTableData, ITransaction>();

        filteredImportedTransactions = importedTransactions.filter(
          (transaction) => {
            const existingTransaction = transactionsInDateRange.find((t) => {
              let doesTransactionMatch =
                filterByDateField.getValue() ||
                filterByDescriptionField.getValue() ||
                filterByCategoryField.getValue() ||
                filterByAmountField.getValue() ||
                filterByAccountField.getValue();

              if (filterByDateField.getValue()) {
                doesTransactionMatch =
                  doesTransactionMatch &&
                  areDatesEqual(t.date, transaction.date);
              }
              if (filterByDescriptionField.getValue()) {
                doesTransactionMatch =
                  doesTransactionMatch &&
                  areStringsEqual(
                    t.merchantName ?? "",
                    transaction.description ?? ""
                  );
              }
              if (filterByCategoryField.getValue()) {
                const importedParentCategory = getParentCategory(
                  transaction.category ?? "",
                  transactionCategoriesWithCustom
                );
                const importedChildCategory = getIsParentCategory(
                  transaction.category ?? "",
                  transactionCategoriesWithCustom
                )
                  ? ""
                  : transaction.category ?? "";
                doesTransactionMatch =
                  doesTransactionMatch &&
                  areStringsEqual(t.category ?? "", importedParentCategory) &&
                  areStringsEqual(t.subcategory ?? "", importedChildCategory);
              }
              if (filterByAmountField.getValue()) {
                doesTransactionMatch =
                  doesTransactionMatch && t.amount === transaction.amount;
              }
              if (filterByAccountField.getValue()) {
                const importedAccountID = accountsQuery.data?.find((account) =>
                  areStringsEqual(account.name, transaction.account ?? "")
                )?.id;

                doesTransactionMatch =
                  doesTransactionMatch &&
                  !!importedAccountID &&
                  areStringsEqual(t.accountID ?? "", importedAccountID);
              }

              return doesTransactionMatch;
            });

            if (existingTransaction) {
              tempDuplicateTransactions.set(transaction, existingTransaction);
              return false;
            }
            return true;
          }
        );

        setDuplicateTransactions(tempDuplicateTransactions);
      } else {
        filteredImportedTransactions = importedTransactions;

        setDuplicateTransactions(
          new Map<ITransactionImportTableData, ITransaction>()
        );
      }

      setImportedTransactionsTableData(filteredImportedTransactions);

      const accountNameToAccountIdMap = new Map<string, string>();

      filteredImportedTransactions.forEach((transaction) => {
        if (
          transaction.account &&
          !accountNameToAccountIdMap.has(transaction.account)
        ) {
          accountNameToAccountIdMap.set(transaction.account, "");
        }
      });

      setAccountNameToAccountIdMap(accountNameToAccountIdMap);
    } finally {
      setIsLoading(false);
    }
  };

  React.useEffect(
    () => buildTableData(),
    [
      dateField.getValue(),
      descriptionField.getValue(),
      categoryField.getValue(),
      amountField.getValue(),
      accountField.getValue(),
      incomeField.getValue(),
      expenseField.getValue(),
      dateFormatField.getValue(),
      invertAmountField.getValue(),
      splitIntoSeparateColumnsField.getValue(),
      includeExpensesColumnField.getValue(),
      expensesColumnField.getValue(),
      expensesColumnValueField.getValue(),
      filterDuplicatesField.getValue(),
      filterByDateField.getValue(),
      filterByDescriptionField.getValue(),
      filterByCategoryField.getValue(),
      filterByAmountField.getValue(),
      filterByAccountField.getValue(),
    ]
  );

  const deleteImportedTransaction = (uid: number) => {
    setCsvData((prev) => prev.filter((row: any) => row.uid !== uid));
    setImportedTransactionsTableData((prev) =>
      prev.filter((row) => row.uid !== uid)
    );
  };

  const restoreFilteredTransactions = (uid: number) => {
    const filteredTransaction = Array.from(duplicateTransactions.keys()).find(
      (transaction) => transaction.uid === uid
    );

    if (!filteredTransaction) {
      return;
    }

    setImportedTransactionsTableData((prev) => [...prev, filteredTransaction]);

    setDuplicateTransactions((prev) => {
      const newMap = new Map(prev);
      newMap.delete(filteredTransaction);
      return newMap;
    });
  };

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
      case "incomeAmount":
        incomeField.setValue(value);
        break;
      case "expenseAmount":
        expenseField.setValue(value);
        break;
      default:
        // This really should be a debug assert.
        notifications.show({
          color: "red",
          message: `Unknown column: ${column}`,
        });
        break;
    }
  };

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

  const filteredImportedData = React.useMemo(() => {
    return importedTransactionsTableData.filter(
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
  }, [importedTransactionsTableData, accountNameToAccountIdMap]);

  const onSubmit = async () => {
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
          <CsvOptions loadCsv={importCsvFile} />
          {importedTransactionsTableData.length > 0 && (
            <TransactionsTable
              tableData={importedTransactionsTableData}
              delete={deleteImportedTransaction}
            />
          )}
          {filterDuplicatesField.getValue() && (
            <DuplicateTransactionTable
              tableData={duplicateTransactions}
              restoreTransaction={restoreFilteredTransactions}
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
              splitAmount={splitIntoSeparateColumnsField.getValue()}
              incomeAmount={incomeField.getValue()}
              expenseAmount={expenseField.getValue()}
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
              filterDuplicates={filterDuplicatesField.getValue()}
              setFilterDuplicates={filterDuplicatesField.setValue}
              filterByDate={filterByDateField.getValue()}
              setFilterByDate={filterByDateField.setValue}
              filterByDescription={filterByDescriptionField.getValue()}
              setFilterByDescription={filterByDescriptionField.setValue}
              filterByCategory={filterByCategoryField.getValue()}
              setFilterByCategory={filterByCategoryField.setValue}
              filterByAmount={filterByAmountField.getValue()}
              setFilterByAmount={filterByAmountField.setValue}
              filterByAccount={filterByAccountField.getValue()}
              setFilterByAccount={filterByAccountField.setValue}
              dateFormat={dateFormatField.getValue() ?? ""}
              setDateFormat={dateFormatField.setValue}
              splitAmountColumn={splitIntoSeparateColumnsField.getValue()}
              setSplitAmountColumn={splitIntoSeparateColumnsField.setValue}
            />
          )}
          {accountNameToAccountIdMap.size > 0 && (
            <AccountMapping
              accountNameToAccountIdMap={accountNameToAccountIdMap}
              setAccountNameToAccountIdMap={setAccountNameToAccountIdMap}
            />
          )}
          {importedTransactionsTableData.length > 0 && (
            <Button
              onClick={onSubmit}
              loading={doImportMutation.isPending}
              disabled={
                doImportMutation.isPending || filteredImportedData.length === 0
              }
            >
              Import {filteredImportedData.length} Transactions
            </Button>
          )}
        </Stack>
      </Modal>
    </>
  );
};

export default ImportTransactionsModal;
