import React from "react";
import { ICategoryResponse } from "~/models/category";
import { defaultGuid } from "~/models/applicationUser";
import { useTransactionCategoriesQuery } from "~/hooks/queries/useTransactionCategoriesQuery";

interface TransactionCategoriesContextType {
  allTransactionCategories: ICategoryResponse[];
  customTransactionCategories: ICategoryResponse[];
}

export const TransactionCategoriesContext =
  React.createContext<TransactionCategoriesContextType>({
    allTransactionCategories: [],
    customTransactionCategories: [],
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

  const value = React.useMemo(
    () => ({
      allTransactionCategories: transactionCategoriesQuery.data ?? [],
      customTransactionCategories,
    }),
    [transactionCategoriesQuery.data, customTransactionCategories],
  );

  return (
    <TransactionCategoriesContext.Provider value={value}>
      {props.children}
    </TransactionCategoriesContext.Provider>
  );
};

export const useTransactionCategories = () =>
  React.useContext(TransactionCategoriesContext);
