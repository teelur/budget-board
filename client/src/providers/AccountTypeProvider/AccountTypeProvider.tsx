import React from "react";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IAccountTypeResponse } from "~/models/accountType";
import { defaultGuid } from "~/models/applicationUser";

interface AccountTypeContextType {
  allAccountTypes: IAccountTypeResponse[];
  customAccountTypes: IAccountTypeResponse[];
}

export const AccountTypeContext = React.createContext<AccountTypeContextType>({
  allAccountTypes: [],
  customAccountTypes: [],
});

interface AccountTypeProviderProps {
  children: React.ReactNode;
}

export const AccountTypeProvider = (props: AccountTypeProviderProps) => {
  const { request } = useAuth();

  const accountTypesQuery = useQuery({
    queryKey: ["accountTypes"],
    queryFn: async (): Promise<IAccountTypeResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/accountType",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountTypeResponse[];
      }

      return [];
    },
  });

  const customAccountTypes = React.useMemo(
    () =>
      accountTypesQuery.data
        ? accountTypesQuery.data.filter((type) => type.id !== defaultGuid)
        : [],
    [accountTypesQuery.data],
  );

  const value = React.useMemo(
    () => ({
      allAccountTypes: accountTypesQuery.data ?? [],
      customAccountTypes: customAccountTypes,
    }),
    [accountTypesQuery.data],
  );

  return (
    <AccountTypeContext.Provider value={value}>
      {props.children}
    </AccountTypeContext.Provider>
  );
};

export const useAccountTypes = () => React.useContext(AccountTypeContext);
