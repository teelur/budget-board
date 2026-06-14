import React from "react";
import { IAccountTypeResponse } from "~/models/accountType";
import { defaultGuid } from "~/models/applicationUser";
import { useAccountTypesQuery } from "~/hooks/queries/useAccountTypesQuery";

interface AccountTypeContextType {
  allAccountTypes: IAccountTypeResponse[];
  customAccountTypes: IAccountTypeResponse[];
  isPending: boolean;
}

export const AccountTypeContext = React.createContext<AccountTypeContextType>({
  allAccountTypes: [],
  customAccountTypes: [],
  isPending: false,
});

interface AccountTypeProviderProps {
  children: React.ReactNode;
}

export const AccountTypeProvider = (props: AccountTypeProviderProps) => {
  const accountTypesQuery = useAccountTypesQuery();

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
      isPending: accountTypesQuery.isPending,
    }),
    [accountTypesQuery.data, accountTypesQuery.isPending],
  );

  return (
    <AccountTypeContext.Provider value={value}>
      {props.children}
    </AccountTypeContext.Provider>
  );
};

export const useAccountTypes = () => React.useContext(AccountTypeContext);
