import { MultiSelect, MultiSelectProps } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AccountSource, IAccountResponse } from "~/models/account";
import { AxiosResponse } from "axios";

export interface AccountSelectInputBaseProps extends MultiSelectProps {
  hideHidden?: boolean;
  filterTypes?: string[];
  manualOnly?: boolean;
  maxSelectedValues?: number;
}

const AccountSelectInputBase = ({
  hideHidden = false,
  filterTypes = [],
  manualOnly = false,
  maxSelectedValues = undefined,
  ...props
}: AccountSelectInputBaseProps): React.ReactNode => {
  const { request } = useAuth();

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const getFilteredAccounts = (): IAccountResponse[] => {
    let filteredAccounts = (accountsQuery.data ?? []).filter(
      (a) => a.deleted === null
    );

    if (hideHidden) {
      filteredAccounts = filteredAccounts.filter((a) => !a.hideAccount);
    }

    if (filterTypes.length > 0) {
      filteredAccounts = filteredAccounts.filter((a) =>
        filterTypes?.includes(a.type)
      );
    }

    if (manualOnly) {
      filteredAccounts = filteredAccounts.filter(
        (a) => a.source === AccountSource.Manual
      );
    }

    return filteredAccounts;
  };

  return (
    <MultiSelect
      data={getFilteredAccounts().map((a) => {
        return { value: a.id, label: a.name };
      })}
      placeholder="Select accounts"
      clearable
      maxValues={maxSelectedValues}
      {...props}
    />
  );
};

export default AccountSelectInputBase;
