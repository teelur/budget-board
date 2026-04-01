import { filterVisibleAccounts } from "~/helpers/accounts";
import { Divider, Stack } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import { IInstitution } from "~/models/institution";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { Filters } from "~/models/transaction";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface InstitutionItemProps {
  institution: IInstitution;
}

const InstitutionItem = (props: InstitutionItemProps): React.ReactNode => {
  const { navigateToTransactions } = useTransactionFilters();
  const sortedFilteredAccounts = filterVisibleAccounts(
    props.institution.accounts
  ).sort((a, b) => a.index - b.index);

  return (
    <Card p={0} w="100%" elevation={2}>
      <Stack gap={0}>
        <Stack px="0.5rem" py="0.25rem">
          <PrimaryText size="md">{props.institution.name}</PrimaryText>
        </Stack>
        <Divider c="var(--elevated-color-border)" my="0.125rem" size="xs" />
        <Stack px="0.5rem" py="0.5rem" gap="0.25rem">
          {sortedFilteredAccounts.map((account: IAccountResponse) => (
            <AccountItem
              key={account.id}
              account={account}
              onClick={() => {
                const filters = new Filters();
                filters.accounts = [account.id];
                navigateToTransactions(filters);
              }}
            />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default InstitutionItem;
