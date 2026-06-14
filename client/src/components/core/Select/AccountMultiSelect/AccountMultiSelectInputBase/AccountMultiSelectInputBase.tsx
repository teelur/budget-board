import { MultiSelect, MultiSelectProps } from "@mantine/core";
import React from "react";
import { AccountSource, IAccountResponse } from "~/models/account";
import { useTranslation } from "react-i18next";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";

export interface AccountMultiSelectInputBaseProps extends MultiSelectProps {
  hideHidden?: boolean;
  filterTypes?: string[];
  manualOnly?: boolean;
  maxSelectedValues?: number;
}

const AccountMultiSelectInputBase = ({
  hideHidden = false,
  filterTypes = [],
  manualOnly = false,
  maxSelectedValues = undefined,
  ...props
}: AccountMultiSelectInputBaseProps): React.ReactNode => {
  const { t } = useTranslation();

  const accountsQuery = useAccountsQuery();

  const getFilteredAccounts = (): IAccountResponse[] => {
    const sortedAccounts = (accountsQuery.data ?? []).sort((a, b) =>
      a.name.localeCompare(b.name),
    );

    let filteredAccounts = sortedAccounts.filter((a) => a.deleted === null);

    if (hideHidden) {
      filteredAccounts = filteredAccounts.filter((a) => !a.hideAccount);
    }

    if (filterTypes.length > 0) {
      filteredAccounts = filteredAccounts.filter((a) =>
        filterTypes?.includes(a.type),
      );
    }

    if (manualOnly) {
      filteredAccounts = filteredAccounts.filter(
        (a) => a.source === AccountSource.Manual,
      );
    }

    return filteredAccounts;
  };

  return (
    <MultiSelect
      data={getFilteredAccounts().map((a) => {
        return { value: a.id, label: a.name };
      })}
      placeholder={t("select_accounts")}
      clearable
      maxValues={maxSelectedValues}
      {...props}
    />
  );
};

export default AccountMultiSelectInputBase;
