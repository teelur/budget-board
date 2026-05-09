import React from "react";
import { ICategoryResponse } from "~/models/category";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { defaultGuid } from "~/models/applicationUser";
import { transactionCategoriesQueryKey } from "~/helpers/requests";

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
  const { request } = useAuth();

  const transactionCategoriesQuery = useQuery({
    queryKey: [transactionCategoriesQueryKey],
    queryFn: async (): Promise<ICategoryResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return [];
    },
  });

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
