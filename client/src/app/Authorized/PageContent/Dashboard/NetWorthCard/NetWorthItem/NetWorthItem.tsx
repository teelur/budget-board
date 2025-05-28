import classes from "./NetWorthItem.module.css";

import { Group, Text } from "@mantine/core";
import { IAccount } from "~/models/account";
import { convertNumberToCurrency } from "~/helpers/currency";
import {
  getAccountsOfTypes,
  sumAccountsTotalBalance,
} from "~/helpers/accounts";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface NetWorthItemProps {
  title: string;
  types?: string[];
  accounts: IAccount[];
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const summedAccountsTotalBalance = sumAccountsTotalBalance(
    props.types
      ? getAccountsOfTypes(props.accounts, props.types)
      : props.accounts
  );

  return (
    <Group className={classes.root} justify="space-between" wrap="nowrap">
      <Text fw={500}>{props.title}</Text>
      {userSettingsQuery.isPending ? null : (
        <Text
          style={{
            color:
              summedAccountsTotalBalance < 0
                ? "var(--mantine-color-red-6)"
                : "var(--mantine-color-green-6)",
            fontWeight: 600,
          }}
        >
          {convertNumberToCurrency(
            summedAccountsTotalBalance,
            true,
            userSettingsQuery.data?.currency ?? "USD"
          )}
        </Text>
      )}
    </Group>
  );
};

export default NetWorthItem;
