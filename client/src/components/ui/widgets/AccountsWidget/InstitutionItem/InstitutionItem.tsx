import classes from "./InstitutionItem.module.css";

import { Stack } from "@mantine/core";
import { HeadingText } from "@teelur/budget-board-ui";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { IAccountResponse } from "~/models/account";
import { IInstitution } from "~/models/institution";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { Filters } from "~/models/transaction";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";

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
      <Stack>
        <HeadingText tone="secondary" level={4} className={classes.title}>
          {props.institution.name}
        </HeadingText>
      </Stack>
      <Stack gap="0.5rem">
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
