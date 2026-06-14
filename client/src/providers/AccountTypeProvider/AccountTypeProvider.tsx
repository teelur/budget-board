import React from "react";
import { notifications } from "@mantine/notifications";
import { useTranslation } from "react-i18next";
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
  const { t } = useTranslation();

  React.useEffect(() => {
    if (accountTypesQuery.isError) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("account_types_query_error"),
      });
    }
  }, [accountTypesQuery.isError, t]);

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
