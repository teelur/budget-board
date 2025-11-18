import classes from "./NetWorthItem.module.css";

import { Group, Text } from "@mantine/core";
import { convertNumberToCurrency } from "~/helpers/currency";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
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

  return (
    <Group className={classes.root} justify="space-between" wrap="nowrap">
      <Text fw={600}>{props.title}</Text>
      {userSettingsQuery.isPending ? null : (
        <Text c={props.totalBalance > 0 ? "green" : "red"} fw={600}>
          {convertNumberToCurrency(
            props.totalBalance,
            true,
            userSettingsQuery.data?.currency ?? "USD"
          )}
        </Text>
      )}
    </Group>
  );
};

export default NetWorthItem;
