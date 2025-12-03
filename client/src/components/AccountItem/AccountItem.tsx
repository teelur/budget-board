import classes from "./AccountItem.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import { Group, Stack } from "@mantine/core";
import { AccountSource, IAccountResponse } from "~/models/account";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "../core/Text/PrimaryText/PrimaryText";
import StatusText from "../core/Text/StatusText/StatusText";
import DimmedText from "../core/Text/DimmedText/DimmedText";

interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
  const { request } = useAuth();

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
        <PrimaryText>{props.account.name}</PrimaryText>
        {userSettingsQuery.isPending ? null : (
          <StatusText amount={props.account.currentBalance}>
            {convertNumberToCurrency(
              props.account.currentBalance,
              true,
              userSettingsQuery.data?.currency ?? "USD"
            )}
          </StatusText>
        )}
      </Group>
      {props.account.source !== AccountSource.Manual && (
        <Group gap="0.25rem">
          <DimmedText size="sm">{"Last updated: "}</DimmedText>
          <DimmedText size="sm">
            {props.account.balanceDate
              ? new Date(props.account.balanceDate).toLocaleString()
              : "Never!"}
          </DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default AccountItem;
