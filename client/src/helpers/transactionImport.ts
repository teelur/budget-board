import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { areStringsEqual } from "~/helpers/utils";
import {
  ITransaction,
  ITransactionImportDuplicateOptions,
  ITransactionImportTableData,
} from "~/models/transaction";
import dayjs from "~/shared/dayjs";

export const normalizeImportedAccountName = (
  accountName: string | null | undefined,
): string => (accountName ?? "").trim().toLowerCase();

export const getMappedAccountId = (
  accountMap: Map<string, string>,
  accountName: string | null | undefined,
): string => {
  const normalizedAccountName = normalizeImportedAccountName(accountName);
  if (!normalizedAccountName) {
    return "";
  }

  for (const [mappedAccountName, accountId] of accountMap) {
    if (
      normalizeImportedAccountName(mappedAccountName) === normalizedAccountName
    ) {
      return accountId;
    }
  }

  return "";
};

interface DuplicateFilterResult {
  filteredTransactions: ITransactionImportTableData[];
  duplicateTransactions: Map<ITransactionImportTableData, ITransaction>;
}

export const filterImportedTransactionDuplicates = (
  importedTransactions: ITransactionImportTableData[],
  existingTransactions: ITransaction[],
  transactionCategories: Parameters<typeof getParentCategory>[1],
  duplicateOptions: ITransactionImportDuplicateOptions,
  accountMap: Map<string, string>,
): DuplicateFilterResult => {
  const duplicateTransactions = new Map<
    ITransactionImportTableData,
    ITransaction
  >();

  if (
    !duplicateOptions.filterDuplicates ||
    importedTransactions.length === 0 ||
    existingTransactions.length === 0
  ) {
    return {
      filteredTransactions: importedTransactions,
      duplicateTransactions,
    };
  }

  const filterOptions = duplicateOptions.filterByOptions;
  if (
    !filterOptions.date &&
    !filterOptions.merchantName &&
    !filterOptions.category &&
    !filterOptions.amount &&
    !filterOptions.account
  ) {
    return {
      filteredTransactions: importedTransactions,
      duplicateTransactions,
    };
  }

  const makeKey = (
    transaction: ITransaction | ITransactionImportTableData,
    isImported: boolean,
  ): string => {
    const parts: string[] = [];
    const importedTransaction = transaction as ITransactionImportTableData;
    const existingTransaction = transaction as ITransaction;
    if (filterOptions.date) {
      parts.push(dayjs(transaction.date).startOf("day").valueOf().toString());
    }
    if (filterOptions.merchantName) {
      parts.push((transaction.merchantName ?? "").trim().toLowerCase());
    }
    if (filterOptions.category) {
      if (isImported) {
        const importedCategory = importedTransaction.category ?? "";
        const isParent = getIsParentCategory(
          importedCategory,
          transactionCategories,
        );
        parts.push(
          getParentCategory(importedCategory, transactionCategories)
            .toString()
            .trim()
            .toLowerCase(),
        );
        parts.push(
          (isParent ? "" : importedCategory).toString().trim().toLowerCase(),
        );
      } else {
        parts.push((existingTransaction.category ?? "").trim().toLowerCase());
        parts.push(
          (existingTransaction.subcategory ?? "").trim().toLowerCase(),
        );
      }
    }
    if (filterOptions.amount) {
      parts.push((transaction.amount ?? "").toString());
    }
    if (filterOptions.account) {
      parts.push(
        isImported
          ? getMappedAccountId(accountMap, importedTransaction.account)
          : existingTransaction.accountID.trim(),
      );
    }
    return parts.join("|");
  };

  const existingIndex = new Map<string, ITransaction[]>();
  for (const transaction of existingTransactions) {
    const key = makeKey(transaction, false);
    const candidates = existingIndex.get(key) ?? [];
    candidates.push(transaction);
    existingIndex.set(key, candidates);
  }

  const filteredTransactions: ITransactionImportTableData[] = [];
  for (const importedTransaction of importedTransactions) {
    const candidates = existingIndex.get(makeKey(importedTransaction, true));
    const matched = candidates?.find((existingTransaction) => {
      if (
        filterOptions.account &&
        !getMappedAccountId(accountMap, importedTransaction.account)
      ) {
        return false;
      }

      return (
        (!filterOptions.date ||
          dayjs(existingTransaction.date).isSame(
            importedTransaction.date,
            "day",
          )) &&
        (!filterOptions.merchantName ||
          areStringsEqual(
            existingTransaction.merchantName ?? "",
            importedTransaction.merchantName ?? "",
          )) &&
        (!filterOptions.category ||
          (areStringsEqual(
            existingTransaction.category ?? "",
            getParentCategory(
              importedTransaction.category ?? "",
              transactionCategories,
            ),
          ) &&
            areStringsEqual(
              existingTransaction.subcategory ?? "",
              getIsParentCategory(
                importedTransaction.category ?? "",
                transactionCategories,
              )
                ? ""
                : (importedTransaction.category ?? ""),
            ))) &&
        (!filterOptions.amount ||
          existingTransaction.amount === importedTransaction.amount) &&
        (!filterOptions.account ||
          areStringsEqual(
            existingTransaction.accountID,
            getMappedAccountId(accountMap, importedTransaction.account),
          ))
      );
    });

    if (matched) {
      duplicateTransactions.set(importedTransaction, matched);
    } else {
      filteredTransactions.push(importedTransaction);
    }
  }

  return { filteredTransactions, duplicateTransactions };
};
