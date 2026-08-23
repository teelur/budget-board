import { Stack, Divider, Group, Button } from "@mantine/core";
import React from "react";
import { filterVisibleAccounts } from "~/helpers/accounts";
import AccountMappingItem from "./AccountMappingItem/AccountMappingItem";
import { ITransactionImportTableData } from "~/models/transaction";
import { MoveLeftIcon } from "lucide-react";
import { getMappedAccountId } from "~/helpers/transactionImport";
import { useTranslation } from "react-i18next";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";

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
  advanceToNextDialog: (
    filteredImportData: ITransactionImportTableData[],
    accountMap: Map<string, string>,
  ) => void;
}

const AccountMapping = (props: AccountMappingProps) => {
  const { t } = useTranslation();

  const accountsQuery = useAccountsQuery();

  const filteredAccounts: IAccountItem[] = filterVisibleAccounts(
    accountsQuery.data ?? [],
  )
    .sort((a, b) => a.name.localeCompare(b.name))
    .map((account) => ({
      value: account.id,
      label: account.name,
    }));

  const filteredImportData = props.importedTransactions
    .filter(
      (t) =>
        getMappedAccountId(props.accountNameToAccountIdMap, t.account) !== "" &&
        getMappedAccountId(props.accountNameToAccountIdMap, t.account) !==
          "exclude",
    )
    .map((transaction) => ({
      ...transaction,
      account: transaction.account?.trim() ?? null,
    }));

  return (
    <Stack gap="0.5rem" w={800} maw="100%" mx="auto">
      <Divider label={t("account_mapping")} labelPosition="center" />
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
        ),
      )}
      <Group w="100%">
        <Button
          flex="1 1 auto"
          onClick={() => props.goBackToPreviousDialog()}
          leftSection={<MoveLeftIcon size={16} />}
        >
          {t("back")}
        </Button>
        <Button
          flex="1 1 auto"
          onClick={() =>
            props.advanceToNextDialog(
              filteredImportData,
              props.accountNameToAccountIdMap,
            )
          }
          disabled={filteredImportData.length === 0}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default AccountMapping;
