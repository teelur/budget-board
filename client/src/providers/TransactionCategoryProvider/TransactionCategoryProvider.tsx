import React from "react";
import { ICategory } from "~/models/category";
import { AuthContext } from "../AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { defaultTransactionCategories } from "~/models/transaction";

interface TransactionCategoriesContextType {
  transactionCategories: ICategory[];
}

export const TransactionCategoriesContext =
  React.createContext<TransactionCategoriesContextType>({
    transactionCategories: [],
  });

interface TransactionCategoriesProviderProps {
  children: React.ReactNode;
}

export const TransactionCategoryProvider = (
  props: TransactionCategoriesProviderProps
) => {
  const [transactionCategories, setTransactionCategories] = React.useState<
    ICategory[]
  >([]);

  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async (): Promise<ICategory[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategory[];
      }

      return [];
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.disableBuiltInTransactionCategories) {
      if (transactionCategoriesQuery.data) {
        setTransactionCategories(transactionCategoriesQuery.data ?? []);
      }
    } else {
      setTransactionCategories(
        defaultTransactionCategories.concat(
          transactionCategoriesQuery.data ?? []
        )
      );
    }
  }, [transactionCategoriesQuery.data, userSettingsQuery.data]);

  const value = React.useMemo(
    () => ({
      transactionCategories,
    }),
    [transactionCategories]
  );

  return (
    <TransactionCategoriesContext.Provider value={value}>
      {props.children}
    </TransactionCategoriesContext.Provider>
  );
};

export const useTransactionCategories = () =>
  React.useContext(TransactionCategoriesContext);
