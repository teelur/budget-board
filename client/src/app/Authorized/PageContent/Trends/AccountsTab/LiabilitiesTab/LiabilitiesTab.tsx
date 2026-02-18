import classes from "./LiabilitiesTab.module.css";

import { Stack } from "@mantine/core";
import React from "react";
import { DatesRangeValue } from "@mantine/dates";
import { mantineDateFormat } from "~/helpers/datetime";
import AccountsSelectHeader from "~/components/AccountsSelectHeader/AccountsSelectHeader";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueries, useQuery } from "@tanstack/react-query";
import { IBalanceResponse } from "~/models/balance";
import { AxiosResponse } from "axios";
import { IAccountResponse } from "~/models/account";
import { IItem } from "~/components/Charts/ValueChart/helpers/valueChart";
import { useDate } from "~/providers/DateProvider/DateProvider";

const LiabilitiesTab = (): React.ReactNode => {
  const { request } = useAuth();
  const { dayjs } = useDate();

  const [selectedAccountIds, setSelectedAccountIds] = React.useState<string[]>(
    [],
  );
  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs().subtract(1, "month").startOf("month").format(mantineDateFormat),
    dayjs().startOf("month").format(mantineDateFormat),
  ]);

  const balancesQuery = useQueries({
    queries: selectedAccountIds.map((accountId: string) => ({
      queryKey: ["balances", accountId],
      queryFn: async (): Promise<IBalanceResponse[]> => {
        const res: AxiosResponse = await request({
          url: "/api/balance",
          method: "GET",
          params: { accountId },
        });

        if (res.status === 200) {
          return res.data as IBalanceResponse[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

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
    <Stack className={classes.root}>
      <AccountsSelectHeader
        selectedAccountIds={selectedAccountIds}
        setSelectedAccountIds={setSelectedAccountIds}
        dateRange={dateRange}
        setDateRange={setDateRange}
        filters={["Loan", "Credit Card", "Mortgage"]}
      />
      <ValueChart
        values={(balancesQuery.data ?? []).map((balance) => ({
          ...balance,
          parentId: balance.accountID || "",
        }))}
        items={(accountsQuery.data ?? [])
          .filter((a) => selectedAccountIds.includes(a.id))
          .map(
            (account) =>
              ({
                id: account.id,
                name: account.name,
              }) as IItem,
          )}
        dateRange={dateRange}
        isPending={balancesQuery.isPending || accountsQuery.isPending}
        invertYAxis
      />
    </Stack>
  );
};

export default LiabilitiesTab;
