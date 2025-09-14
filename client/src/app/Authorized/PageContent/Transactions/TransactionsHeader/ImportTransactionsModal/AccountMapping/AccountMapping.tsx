import { Stack, Divider, Group, Button } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { IAccount } from "~/models/account";
import AccountMappingItem from "./AccountMappingItem/AccountMappingItem";
import { ITransactionImportTableData } from "~/models/transaction";
import { MoveLeftIcon } from "lucide-react";
import { areStringsEqual } from "~/helpers/utils";

export interface IAccountItem {
  value: string;
  label: string;
}

interface AccountMappingProps {
  importedTransactions: ITransactionImportTableData[];
  accountNameToAccountIdMap: Map<string, string>;
  setAccountNameToAccountIdMap: React.Dispatch<
    React.SetStateAction<Map<string, string>>
  >;
  goBackToPreviousDialog: () => void;
  submitImport: (filteredImportData: ITransactionImportTableData[]) => void;
  isSubmitting?: boolean;
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

  const filteredImportData = props.importedTransactions.filter(
    (t) =>
      !areStringsEqual(
        props.accountNameToAccountIdMap.get(t.account ?? "") ?? "",
        "exclude"
      ) &&
      !areStringsEqual(
        props.accountNameToAccountIdMap.get(t.account ?? "") ?? "",
        ""
      )
  );

  return (
    <Stack gap="0.5rem" miw={400} maw="100%">
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
          onClick={() => props.submitImport(filteredImportData)}
          loading={props.isSubmitting}
          disabled={filteredImportData.length === 0}
        >
          Import {filteredImportData.length} Transactions
        </Button>
      </Group>
    </Stack>
  );
};

export default AccountMapping;
