import { Stack, Divider } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { IAccount } from "~/models/account";
import AccountMappingItem from "./AccountMappingItem/AccountMappingItem";

export interface IAccountItem {
  value: string;
  label: string;
}

interface AccountMappingProps {
  accountNameToAccountIdMap: Map<string, string>;
  setAccountNameToAccountIdMap: React.Dispatch<
    React.SetStateAction<Map<string, string>>
  >;
}

const AccountMapping = (props: AccountMappingProps) => {
  const { request } = React.useContext<any>(AuthContext);

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

  const filteredAccounts: IAccountItem[] = filterVisibleAccounts(
    accountsQuery.data ?? []
  )
    .sort((a, b) => a.name.localeCompare(b.name))
    .map((account) => ({
      value: account.id,
      label: account.name,
    }));

  return (
    <Stack>
      <Divider label="Account Mapping" labelPosition="center" />
      {Array.from(props.accountNameToAccountIdMap.entries()).map(
        ([accountName, accountId]) => (
          <AccountMappingItem
            key={accountName}
            accountName={accountName}
            accountId={accountId}
            accounts={filteredAccounts}
            onAccountChange={(name, id) =>
              props.setAccountNameToAccountIdMap((prev) => {
                const newMap = new Map(prev);
                newMap.set(name, id);
                return newMap;
              })
            }
          />
        )
      )}
    </Stack>
  );
};

export default AccountMapping;
