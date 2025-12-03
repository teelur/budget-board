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
    <Card w="100%" elevation={2}>
      <Stack gap="0.25rem">
        <PrimaryText size="md">{props.institution.name}</PrimaryText>
        <Divider
          c="var(--elevated-color-border)"
          variant="dotted"
          mb="xs"
          size="sm"
        />
      </Stack>
      <Stack gap={1}>
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
    </Card>
  );
};

export default InstitutionItem;
