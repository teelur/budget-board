import AccountSelectInput from "~/components/AccountSelectInput";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { Button, Group } from "@mantine/core";
import { DatePickerInput, DatesRangeValue } from "@mantine/dates";
import { IAccountResponse } from "~/models/account";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";

interface AccountsSelectHeaderProps {
  selectedAccountIds: string[];
  setSelectedAccountIds: (accountIds: string[]) => void;
  dateRange: DatesRangeValue<string>;
  setDateRange: (dateRange: DatesRangeValue<string>) => void;
  filters?: string[];
}

const AccountsSelectHeader = (
  props: AccountsSelectHeaderProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);
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

  return (
    <Group>
      <DatePickerInput
        type="range"
        value={props.dateRange}
        onChange={props.setDateRange}
      />
      <AccountSelectInput
        selectedAccountIds={props.selectedAccountIds}
        setSelectedAccountIds={props.setSelectedAccountIds}
        hideHidden
        filterTypes={props.filters}
        miw="230px"
        maw="400px"
      />
      <Button
        onClick={() => {
          props.setSelectedAccountIds(
            accountsQuery.data
              ?.filter(
                (account: IAccountResponse) =>
                  !account.hideAccount &&
                  !account.deleted &&
                  (props.filters ? props.filters?.includes(account.type) : true)
              )
              ?.map((account) => account.id) ?? []
          );
        }}
      >
        Select All
      </Button>
      <Button onClick={() => props.setSelectedAccountIds([])}>Clear All</Button>
    </Group>
  );
};

export default AccountsSelectHeader;
