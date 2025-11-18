import { Alert, Button, Group, Stack } from "@mantine/core";
import React from "react";
import TransactionsTable from "./TransactionsTable/TransactionsTable";
import {
  ITransaction,
  ITransactionImportTableData,
} from "~/models/transaction";
import { CsvRow } from "../LoadCsv/LoadCsv";
import ColumnsOptions, {
  dateFormatOptions,
  IColumnsOptions,
} from "./ColumnsOptions/ColumnsOptions";
import { areStringsEqual } from "~/helpers/utils";
import ColumnsSelect, { ISelectedColumns } from "./ColumnsSelect/ColumnsSelect";
import DuplicateTransactionTable from "./DuplicateTransactionTable/DuplicateTransactionTable";
import { notifications } from "@mantine/notifications";
import dayjs from "dayjs";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { IAccountResponse } from "~/models/account";
import { InfoIcon, MoveLeftIcon, MoveRightIcon } from "lucide-react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface ConfigureTransactionsProps {
  csvData: CsvRow[];
  csvHeaders: string[];
  advanceToNextDialog: (data: ITransactionImportTableData[]) => void;
  goBackToPreviousDialog: () => void;
}

const ConfigureTransactions = (
  props: ConfigureTransactionsProps
): React.ReactNode => {
  const [isPending, startTransition] = React.useTransition();

  const [alertDetails, setAlertDetails] = React.useState<string | null>(null);

  // The raw CSV data imported from the user's file.
  const [csvData, setCsvData] = React.useState<CsvRow[]>(props.csvData);

  // The imported transactions table data built from `csvData` and the current
  // column selections and options.
  const [importedTransactionsTableData, setImportedTransactionsTableData] =
    React.useState<ITransactionImportTableData[]>([]);

  // Column selections made by the user to map CSV columns to transaction fields.
  const [columnsSelect, setColumnsSelect] = React.useState<ISelectedColumns>({
    date:
      props.csvHeaders.find((header) =>
        areStringsEqual(header.toLowerCase(), "date")
      ) ?? null,
    description:
      props.csvHeaders.find((header) =>
        areStringsEqual(header.toLowerCase(), "description")
      ) ?? null,
    category:
      props.csvHeaders.find((header) =>
        areStringsEqual(header.toLowerCase(), "category")
      ) ?? null,
    amount:
      props.csvHeaders.find((header) =>
        areStringsEqual(header.toLowerCase(), "amount")
      ) ?? null,
    account:
      props.csvHeaders.find((header) =>
        areStringsEqual(header.toLowerCase(), "account")
      ) ?? null,
    incomeAmount: null,
    expenseAmount: null,
  });

  // Options for parsing and interpreting CSV columns.
  const [columnsOptions, setColumnsOptions] = React.useState<IColumnsOptions>({
    dateFormat: dateFormatOptions[0]!.value,
    invertAmount: false,
    splitAmountColumn: false,
    includeExpensesColumn: false,
    expensesColumn: null,
    expensesColumnValue: null,
    filterDuplicates: false,
    filterByOptions: {
      date: false,
      description: false,
      category: false,
      amount: false,
      account: false,
    },
  });

  // Transactions detected as potential duplicates of existing transactions.
  const [duplicateTransactions, setDuplicateTransactions] = React.useState<
    Map<ITransactionImportTableData, ITransaction>
  >(new Map<ITransactionImportTableData, ITransaction>());

  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();

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
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const disableNextButton = () => {
    // Cannot proceed if there are no imported transactions.
    if (importedTransactionsTableData.length === 0) {
      setAlertDetails("No transactions to import.");
      return;
    }

    // The account column is required to add transactions.
    if (!columnsSelect.account) {
      setAlertDetails("Account column is required.");
      return;
    }

    if (
      !columnsSelect.date &&
      !columnsSelect.description &&
      !columnsSelect.category
    ) {
      if (columnsOptions.splitAmountColumn) {
        if (!columnsSelect.incomeAmount && !columnsSelect.expenseAmount) {
          setAlertDetails(
            "One or more of Date, Description, Category, Income Amount, or Expense Amount columns are required."
          );
          return;
        }
      } else if (!columnsSelect.amount) {
        setAlertDetails(
          "One or more of Date, Description, Category, or Amount columns are required."
        );
        return;
      }
    }

    setAlertDetails(null);
  };

  React.useEffect(() => {
    disableNextButton();
  }, [
    importedTransactionsTableData,
    columnsSelect,
    columnsOptions.splitAmountColumn,
  ]);

  /**
   * Extracts the unique, non-null string values from the provided CSV data for a given column.
   *
   * @param column - The column name (key) to extract values from each CSV row.
   * @returns An array of unique values from the column, converted to strings.
   *
   * @example
   * // given csvData = [{ Category: 'Food' }, { Category: 'Rent' }, { Category: 'Food' }]
   * // getExpensesColumnValues('Category') -> ['Food', 'Rent']
   */
  const getExpensesColumnValues = (column: string): string[] => {
    const columnValues = csvData.map((row: any) => {
      if (column.length === 0) {
        return null;
      }

      const value = row[column];
      if (value) {
        return value;
      }
      return null;
    });

    const uniqueValues = Array.from(new Set(columnValues)).filter(
      (value) => value !== null
    );
    return uniqueValues.map((value) => value.toString());
  };

  /**
   * Remove an imported transaction from component state by its unique identifier.
   *
   * Immutably updates both `csvData` and `importedTransactionsTableData` by filtering out
   * any rows whose `uid` matches the provided value. If no matching row is found, the
   * state values remain unchanged.
   *
   * @param uid - The numeric unique identifier of the imported transaction to remove.
   * @returns void
   *
   * @remarks
   * - Assumes rows in both `csvData` and `importedTransactionsTableData` have a `uid` property.
   * - Safe to call as an event handler (performs no synchronous side effects beyond state updates).
   */
  const deleteImportedTransaction = (uid: number) => {
    setCsvData((prev) => prev.filter((row: any) => row.uid !== uid));
    setImportedTransactionsTableData((prev) =>
      prev.filter((row) => row.uid !== uid)
    );
  };

  /**
   * Restores a previously flagged duplicate transaction back into the imported transactions table.
   *
   * Searches the keys of the `duplicateTransactions` Map for a transaction whose `uid` matches the
   * provided `uid`. When found, the transaction is appended to the `importedTransactionsTableData`
   * state and removed from the `duplicateTransactions` state.
   *
   * State updates are performed immutably:
   * - `importedTransactionsTableData` is updated by creating a new array with the restored transaction appended.
   * - `duplicateTransactions` is updated by creating a shallow copy of the previous Map and deleting the restored key.
   *
   * If no matching transaction is found, the function returns early and does not modify state.
   *
   * @param uid - Unique identifier of the transaction to restore.
   * @returns void
   */
  const restoreImportedTransaction = (uid: number) => {
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

  /**
   * Extracts and returns a numeric transaction amount from a parsed CSV row according to the current
   * column-selection options.
   *
   * Side effects:
   * - May call notifications.show(...) when a row contains both income and expense values.
   *
   * Notes:
   * - Non-numeric or missing values result in a return value of null.
   * - The function depends on external configuration objects (columnsSelect, columnsOptions) and a notifications
   *   mechanism to display warnings.
   *
   * @param row - A CSV row object (expected to contain string values for configured columns and a uid used in notifications).
   * @returns The parsed amount as a number (positive for income, negative for expense) after applying any configured inversion,
   *          or null if no valid numeric amount could be determined.
   */
  const getImportedTransactionAmount = (row: CsvRow): number | null => {
    // User can select an option to invert the amount values during import.
    const applyInversion = (amount: number) => {
      if (columnsOptions.invertAmount) {
        return amount * -1;
      }
      return amount;
    };

    if (columnsOptions.splitAmountColumn) {
      const incomeValue: string | null = columnsSelect.incomeAmount
        ? (row[columnsSelect.incomeAmount] as string)
        : null;
      const expenseValue: string | null = columnsSelect.expenseAmount
        ? (row[columnsSelect.expenseAmount] as string)
        : null;

      if (incomeValue && expenseValue) {
        notifications.show({
          color: "yellow",
          message: `Row has both income and expense values, defaulting to income. Row UID: ${row.uid}`,
        });
      }

      if (incomeValue) {
        const parsed = parseFloat(incomeValue);
        return Number.isNaN(parsed)
          ? null
          : applyInversion(parseFloat(incomeValue));
      }
      if (expenseValue) {
        const parsed = parseFloat(expenseValue);
        return Number.isNaN(parsed) ? null : applyInversion(parsed * -1);
      }
    } else {
      const amountValue: string | null = columnsSelect.amount
        ? (row[columnsSelect.amount] as string)
        : null;
      if (amountValue) {
        const parsed = parseFloat(amountValue);
        return Number.isNaN(parsed) ? null : applyInversion(parsed);
      }
    }
    return null;
  };

  /**
   * Parse imported CSV rows into the application's transaction import table shape.
   *
   * Iterates over the `csvData` rows and maps each row into an `ITransactionImportTableData`
   * object using the currently selected column mappings and parsing options.
   *
   * Returns:
   *   An array of `ITransactionImportTableData` objects corresponding to each CSV row.
   *
   * Notes:
   * - This function relies on external state: `csvData`, `columnsSelect`, `columnsOptions`,
   *   and `getImportedTransactionAmount`.
   * - It performs no validation beyond the described mappings; callers should validate
   *   or normalize values (e.g., ensure dates are valid) as needed.
   */
  const parseImportedTransactions = (): ITransactionImportTableData[] =>
    csvData.map((row: any) => {
      return {
        uid: row.uid,
        date: columnsSelect.date
          ? dayjs(row[columnsSelect.date], columnsOptions.dateFormat).toDate()
          : null,
        description: columnsSelect.description
          ? row[columnsSelect.description]
          : null,
        category: columnsSelect.category ? row[columnsSelect.category] : null,
        amount: getImportedTransactionAmount(row),
        account: columnsSelect.account ? row[columnsSelect.account] : null,
        type:
          columnsOptions.includeExpensesColumn &&
          columnsOptions.expensesColumn &&
          columnsOptions.expensesColumn.length > 0
            ? row[columnsOptions.expensesColumn]
            : null,
      } as ITransactionImportTableData;
    });

  /**
   * Applies the configured "expenses" column rule to a list of imported transactions.
   *
   * When the expenses column feature is enabled and a non-empty column identifier is provided,
   * this function iterates the provided transactions and, for each transaction whose type
   * (converted to string) equals the configured expenses column value and which has a truthy amount,
   * it negates the transaction.amount in-place (amount *= -1).
   *
   * Important details:
   * - Mutates the objects in the transactions array directly.
   * - Comparison uses transaction.type.toString() === columnsOptions.expensesColumn.
   * - Only transactions with a truthy amount are modified (0, null, undefined are ignored).
   * - Negation will flip the sign of any existing amount (positive -> negative, negative -> positive).
   *
   * @param transactions - Array of transaction import rows to apply the expenses rule to.
   * @returns void
   */
  const applyExpensesColumn = (transactions: ITransactionImportTableData[]) => {
    if (
      columnsOptions.includeExpensesColumn &&
      (columnsOptions.expensesColumn?.length ?? 0) > 0 &&
      (columnsOptions.expensesColumnValue?.length ?? 0) > 0
    ) {
      transactions.forEach((transaction) => {
        if (
          transaction.type != null &&
          areStringsEqual(
            transaction.type.toString(),
            columnsOptions.expensesColumnValue!
          ) &&
          transaction.amount
        ) {
          transaction.amount *= -1;
        }
      });
    }
  };

  /**
   * Filters out imported transactions that appear to be duplicates of existing transactions,
   * based on the enabled comparison criteria in `columnsOptions.filterByOptions`.
   *
   * Side effects:
   *  - Calls `setDuplicateTransactions` with a Map<ITransactionImportTableData, ITransaction>
   *    representing found duplicates (cleared when there are no candidates).
   *
   * Notes / assumptions:
   *  - `transactions` elements and existing `transactionsQuery.data` elements are expected to
   *    have parsable `date` fields compatible with dayjs.
   *  - `columnsOptions.filterByOptions` controls which fields are used for duplicate detection.
   *  - `accountsQuery.data` is used to resolve account names to ids for account-based matching.
   *
   * @param transactions - Imported transactions to filter for duplicates (ITransactionImportTableData[]).
   * @returns An array of imported transactions with duplicates removed (ITransactionImportTableData[]).
   */
  const filterDuplicates = (transactions: ITransactionImportTableData[]) => {
    // Fast-paths
    if (transactions.length === 0 || !transactionsQuery.data?.length) {
      setDuplicateTransactions(
        new Map<ITransactionImportTableData, ITransaction>()
      );
      return transactions;
    }

    const filterOpts = columnsOptions.filterByOptions;
    if (
      !filterOpts?.date &&
      !filterOpts?.description &&
      !filterOpts?.category &&
      !filterOpts?.amount &&
      !filterOpts?.account
    ) {
      setDuplicateTransactions(
        new Map<ITransactionImportTableData, ITransaction>()
      );
      return transactions;
    }

    // Precompute helper maps to avoid repeated work in inner loop
    const accountNameToId = new Map<string, string>();
    if (accountsQuery.data) {
      for (const a of accountsQuery.data) {
        if (a.name) {
          accountNameToId.set(a.name.trim().toLowerCase(), a.id ?? "");
        }
      }
    }

    // Build an index of existing transactions by a composite key. The key includes
    // date (startOf day) and any enabled comparison fields. This turns O(N*M) finds
    // into O(N + M) map lookups.
    type Key = string;
    const makeKey = (
      t: Partial<ITransaction> | ITransactionImportTableData
    ): Key => {
      const parts: string[] = [];
      if (filterOpts.date && (t as any).date) {
        const d = dayjs((t as any).date)
          .startOf("day")
          .valueOf();
        parts.push(String(d));
      }
      if (filterOpts.description) {
        parts.push(
          ((t as any).merchantName ?? (t as any).description ?? "")
            .toString()
            .trim()
            .toLowerCase()
        );
      }
      if (filterOpts.category) {
        // For existing transactions we expect `category` + `subcategory` fields,
        // for imported rows we derive parent/child from the imported value.
        if (
          (t as ITransaction).category !== undefined ||
          (t as ITransaction).subcategory !== undefined
        ) {
          parts.push(
            ((t as ITransaction).category ?? "").toString().trim().toLowerCase()
          );
          parts.push(
            ((t as ITransaction).subcategory ?? "")
              .toString()
              .trim()
              .toLowerCase()
          );
        } else {
          const importedParent = getParentCategory(
            (t as any).category ?? "",
            transactionCategories
          )
            .toString()
            .trim()
            .toLowerCase();
          const isParent = getIsParentCategory(
            (t as any).category ?? "",
            transactionCategories
          );
          const importedChild = isParent ? "" : (t as any).category ?? "";
          parts.push(importedParent);
          parts.push(importedChild.toString().trim().toLowerCase());
        }
      }
      if (filterOpts.amount) {
        parts.push(String((t as any).amount ?? ""));
      }
      if (filterOpts.account) {
        if ((t as any).accountID !== undefined) {
          parts.push(((t as any).accountID ?? "").toString().trim());
        } else {
          parts.push(
            (
              accountNameToId.get(
                ((t as any).account ?? "").toString().trim().toLowerCase()
              ) ?? ""
            )
              .toString()
              .trim()
          );
        }
      }
      return parts.join("|");
    };

    const existingIndex = new Map<Key, ITransaction[]>();
    for (const ex of transactionsQuery.data) {
      const key = makeKey(ex as any);
      if (!existingIndex.has(key)) {
        existingIndex.set(key, []);
      }
      existingIndex.get(key)!.push(ex);
    }

    const tempDuplicateTransactions = new Map<
      ITransactionImportTableData,
      ITransaction
    >();
    const filtered = [] as ITransactionImportTableData[];

    for (const imp of transactions) {
      const key = makeKey(imp as any);
      const candidates = existingIndex.get(key);
      let matched: ITransaction | undefined;
      if (candidates && candidates.length > 0) {
        for (const c of candidates) {
          let ok = true;
          if (filterOpts.date) {
            ok = ok && dayjs(c.date).isSame(imp.date, "day");
          }
          if (ok && filterOpts.description) {
            ok =
              ok &&
              areStringsEqual(c.merchantName ?? "", imp.description ?? "");
          }
          if (ok && filterOpts.category) {
            const importedParent = getParentCategory(
              imp.category ?? "",
              transactionCategories
            );
            const isParent = getIsParentCategory(
              imp.category ?? "",
              transactionCategories
            );
            const importedChild = isParent ? "" : imp.category ?? "";
            ok =
              ok &&
              areStringsEqual(c.category ?? "", importedParent) &&
              areStringsEqual(c.subcategory ?? "", importedChild);
          }
          if (ok && filterOpts.amount) {
            ok = ok && c.amount === imp.amount;
          }
          if (ok && filterOpts.account) {
            const importedAccountID = accountNameToId.get(
              ((imp.account ?? "") as string).toString().trim().toLowerCase()
            );
            ok =
              ok &&
              !!importedAccountID &&
              areStringsEqual(c.accountID ?? "", importedAccountID ?? "");
          }
          if (ok) {
            matched = c;
            break;
          }
        }
      }

      if (matched) {
        tempDuplicateTransactions.set(imp, matched);
      } else {
        filtered.push(imp);
      }
    }

    setDuplicateTransactions(tempDuplicateTransactions);
    return filtered;
  };

  /**
   * Builds and updates the imported transactions table data from the current column mappings and options.
   *
   * Side effects:
   * - Calls setImportedTransactionsTableData(filteredImportedTransactions ?? []).
   * - Calls setDuplicateTransactions(...) to reset duplicate tracking when appropriate.
   *
   * Dependencies:
   * - Uses external closures / helpers: columnsSelect, columnsOptions, parseImportedTransactions, applyExpensesColumn,
   *   filterDuplicates, setImportedTransactionsTableData, setDuplicateTransactions.
   *
   * Returns:
   * - void
   */
  const buildTableData = () => {
    try {
      // When no columns are mapped, we want to clear out all data, so we can start
      // fresh when the user maps new columns.
      if (
        (!columnsSelect.date &&
          !columnsSelect.description &&
          !columnsSelect.category &&
          !columnsSelect.amount &&
          !columnsSelect.account) ||
        (columnsOptions.splitAmountColumn &&
          !columnsSelect.incomeAmount &&
          !columnsSelect.expenseAmount)
      ) {
        setImportedTransactionsTableData([]);
        setDuplicateTransactions(
          new Map<ITransactionImportTableData, ITransaction>()
        );
      }

      const importedTransactions: ITransactionImportTableData[] =
        parseImportedTransactions();

      // Expense transactions can be indicated in a separate column.
      applyExpensesColumn(importedTransactions);

      let filteredImportedTransactions;
      if (columnsOptions.filterDuplicates) {
        filteredImportedTransactions = filterDuplicates(importedTransactions);
      } else {
        filteredImportedTransactions = importedTransactions;

        setDuplicateTransactions(
          new Map<ITransactionImportTableData, ITransaction>()
        );
      }

      setImportedTransactionsTableData(filteredImportedTransactions ?? []);
    } catch (e) {
      notifications.show({
        color: "red",
        message: `Error building table data: ${
          e instanceof Error ? e.message : String(e)
        }`,
      });
      setImportedTransactionsTableData([]);
      setDuplicateTransactions(
        new Map<ITransactionImportTableData, ITransaction>()
      );
    }
  };

  React.useEffect(() => {
    startTransition(() => {
      buildTableData();
    });
  }, [csvData, columnsSelect, columnsOptions, columnsOptions.filterByOptions]);

  /**
   * Applies new columns options and rebuilds the table data.
   *
   * @param columnsOptions - The updated columns options to apply.
   */
  const applyColumnsOptions = (columnsOptions: IColumnsOptions) => {
    setColumnsOptions(columnsOptions);
  };

  /**
   * Applies new column selections and rebuilds the table data.
   *
   * @param columnsSelect - The updated column selections to apply.
   */
  const applyColumnsSelect = (columnsSelect: ISelectedColumns) => {
    setColumnsSelect(columnsSelect);
  };

  return (
    <Stack gap="0.5rem" w="auto" maw="100%">
      {alertDetails && (
        <Alert
          variant="outline"
          color="red"
          title="Missing Info"
          icon={<InfoIcon />}
          radius="md"
        >
          {alertDetails}
        </Alert>
      )}
      {importedTransactionsTableData.length > 0 && (
        <TransactionsTable
          tableData={importedTransactionsTableData}
          delete={deleteImportedTransaction}
        />
      )}
      {columnsOptions.filterDuplicates && (
        <DuplicateTransactionTable
          tableData={duplicateTransactions}
          restoreTransaction={restoreImportedTransaction}
        />
      )}
      {props.csvHeaders.length > 0 && (
        <ColumnsSelect
          csvHeaders={props.csvHeaders}
          selectedColumns={columnsSelect}
          applySelectedColumns={applyColumnsSelect}
          isAmountSplit={columnsOptions.splitAmountColumn}
        />
      )}
      {props.csvHeaders.length > 0 && (
        <ColumnsOptions
          columnsOptions={columnsOptions}
          applyColumnsOptions={applyColumnsOptions}
          columns={props.csvHeaders}
          getExpensesColumnValues={getExpensesColumnValues}
          loading={isPending}
        />
      )}
      <Group w="100%">
        <Button
          flex="1 1 auto"
          onClick={() => props.goBackToPreviousDialog()}
          leftSection={<MoveLeftIcon size={16} />}
        >
          Back
        </Button>
        <Button
          flex="1 1 auto"
          disabled={alertDetails !== null || isPending}
          onClick={() =>
            props.advanceToNextDialog(importedTransactionsTableData)
          }
          rightSection={<MoveRightIcon size={16} />}
        >
          Next
        </Button>
      </Group>
    </Stack>
  );
};

export default ConfigureTransactions;
