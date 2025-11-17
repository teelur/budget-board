import classes from "./AccountItem.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import { Group, Stack, Text } from "@mantine/core";
import { AccountSource, IAccountResponse } from "~/models/account";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
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
    <Stack
      gap={0.5}
      className={props.onClick ? classes.root : undefined}
      onClick={props.onClick}
    >
      <Group justify="space-between" wrap="nowrap">
        <Text fw={600}>{props.account.name}</Text>
        {userSettingsQuery.isPending ? null : (
          <Text c={props.account.currentBalance < 0 ? "red" : "green"} fw={600}>
            {convertNumberToCurrency(
              props.account.currentBalance,
              true,
              userSettingsQuery.data?.currency ?? "USD"
            )}
          </Text>
        )}
      </Group>
      {props.account.source !== AccountSource.Manual && (
        <Text c="dimmed" fw={600} size="sm">
          {"Last updated: "}
          {props.account.balanceDate
            ? new Date(props.account.balanceDate).toLocaleString()
            : "Never!"}
        </Text>
      )}
    </Stack>
  );
};

export default AccountItem;
