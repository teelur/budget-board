import classes from "./LiabilitiesTab.module.css";

import { Stack } from "@mantine/core";
import React from "react";
import { DatesRangeValue } from "@mantine/dates";
import { getDateFromMonthsAgo, mantineDateFormat } from "~/helpers/datetime";
import AccountsSelectHeader from "~/components/AccountsSelectHeader/AccountsSelectHeader";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueries, useQuery } from "@tanstack/react-query";
import { IBalance } from "~/models/balance";
import { AxiosResponse } from "axios";
import { IAccountResponse } from "~/models/account";
import dayjs from "dayjs";
import { IItem } from "~/components/Charts/ValueChart/helpers/valueChart";

const LiabilitiesTab = (): React.ReactNode => {
  const [selectedAccountIds, setSelectedAccountIds] = React.useState<string[]>(
    []
  );
  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs(getDateFromMonthsAgo(1)).format(mantineDateFormat),
    dayjs().format(mantineDateFormat),
  ]);

  const { request } = React.useContext<any>(AuthContext);
  const balancesQuery = useQueries({
    queries: selectedAccountIds.map((accountId: string) => ({
      queryKey: ["balances", accountId],
      queryFn: async (): Promise<IBalance[]> => {
        const res: AxiosResponse = await request({
          url: "/api/balance",
          method: "GET",
          params: { accountId },
        });

        if (res.status === 200) {
          return res.data as IBalance[];
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
              } as IItem)
          )}
        dateRange={dateRange}
        isPending={balancesQuery.isPending || accountsQuery.isPending}
        invertYAxis
      />
    </Stack>
  );
};

export default LiabilitiesTab;
