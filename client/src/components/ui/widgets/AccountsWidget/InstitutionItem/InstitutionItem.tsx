import classes from "./InstitutionItem.module.css";

import { filterVisibleAccounts } from "~/helpers/accounts";
import { Stack } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import { IInstitution } from "~/models/institution";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { Filters } from "~/models/transaction";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import SecondaryHeading from "~/components/core/Heading/SecondaryHeading/SecondaryHeading";

interface InstitutionItemProps {
  institution: IInstitution;
}

const InstitutionItem = (props: InstitutionItemProps): React.ReactNode => {
  const { navigateToTransactions } = useTransactionFilters();
  const sortedFilteredAccounts = filterVisibleAccounts(
    props.institution.accounts,
  ).sort((a, b) => a.index - b.index);

  return (
    <Stack gap={0} justify="center">
      <Stack px="0.5rem">
        <SecondaryHeading order={4} className={classes.title}>
          {props.institution.name}
        </SecondaryHeading>
      </Stack>
      <Stack px="0.5rem" py="0.25rem" gap="0.5rem">
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
  );
};

export default InstitutionItem;
