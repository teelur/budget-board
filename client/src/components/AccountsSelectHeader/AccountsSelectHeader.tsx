import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { Button, Group } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { IAccountResponse } from "~/models/account";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import DatePickerInput from "../core/Input/DatePickerInput/DatePickerInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "../core/Select/AccountMultiSelect/AccountMultiSelect";

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
  const { t } = useTranslation();
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

  return (
    <Group>
      <DatePickerInput
        type="range"
        value={props.dateRange}
        onChange={props.setDateRange}
        elevation={1}
      />
      <AccountMultiSelect
        value={props.selectedAccountIds}
        onChange={props.setSelectedAccountIds}
        hideHidden
        filterTypes={props.filters}
        miw="230px"
        maw="400px"
        elevation={1}
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
        {t("select_all")}
      </Button>
      <Button onClick={() => props.setSelectedAccountIds([])}>
        {t("clear_all")}
      </Button>
    </Group>
  );
};

export default AccountsSelectHeader;
