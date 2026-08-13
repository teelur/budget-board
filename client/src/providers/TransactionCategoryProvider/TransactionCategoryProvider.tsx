import React from "react";
import {
  CategoryType,
  CategoryTypes,
  ICategoryResponse,
} from "~/models/category";
import { defaultGuid } from "~/models/applicationUser";
import { useTransactionCategoriesQuery } from "~/hooks/queries/useTransactionCategoriesQuery";
import { areStringsEqual } from "~/helpers/utils";

interface TransactionCategoriesContextType {
  allTransactionCategories: ICategoryResponse[];
  customTransactionCategories: ICategoryResponse[];
  getCategoryType: (category: string) => CategoryType | "";
  isPending: boolean;
}

export const TransactionCategoriesContext =
  React.createContext<TransactionCategoriesContextType>({
    allTransactionCategories: [],
    customTransactionCategories: [],
    getCategoryType: () => "",
    isPending: false,
  });

interface TransactionCategoriesProviderProps {
  children: React.ReactNode;
}

export const TransactionCategoryProvider = (
  props: TransactionCategoriesProviderProps,
) => {
  const transactionCategoriesQuery = useTransactionCategoriesQuery();

  const customTransactionCategories = React.useMemo(
    () =>
      transactionCategoriesQuery.data
        ? transactionCategoriesQuery.data.filter(
            (category) => category.id !== defaultGuid,
          )
        : [],
    [transactionCategoriesQuery.data],
  );

  const getCategoryType = React.useCallback(
    (category: string): CategoryType | "" => {
      if (!transactionCategoriesQuery.data) {
        return "";
      }

      // Uncategorized transactions are considered expenses by default
      if (category.length === 0) {
        return CategoryTypes.Expense;
      }

      const foundCategory = transactionCategoriesQuery.data.find((c) =>
        areStringsEqual(c.value, category),
      );

      if (foundCategory?.categoryType === CategoryTypes.Income) {
        return CategoryTypes.Income;
      }

      if (foundCategory?.categoryType === CategoryTypes.Expense) {
        return CategoryTypes.Expense;
      }

      return "";
    },
    [transactionCategoriesQuery.data],
  );

  const value = React.useMemo(
    () => ({
      allTransactionCategories: transactionCategoriesQuery.data ?? [],
      customTransactionCategories,
      isPending: transactionCategoriesQuery.isPending,
      getCategoryType,
    }),
    [
      transactionCategoriesQuery.data,
      transactionCategoriesQuery.isPending,
      customTransactionCategories,
      getCategoryType,
    ],
  );

  return (
    <TransactionCategoriesContext.Provider value={value}>
      {props.children}
    </TransactionCategoriesContext.Provider>
  );
};

export const useTransactionCategories = () =>
  React.useContext(TransactionCategoriesContext);
