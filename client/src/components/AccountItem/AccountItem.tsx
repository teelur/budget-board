import classes from "./AccountItem.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import { Group, Stack } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "../core/Text/PrimaryText/PrimaryText";
import StatusText from "../core/Text/StatusText/StatusText";
import DimmedText from "../core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const { dayjs, dateFormat } = useDate();

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
              userSettingsQuery.data?.currency ?? "USD",
            )}
          </StatusText>
        )}
      </Group>
      <DimmedText size="xs">
        {t("last_updated", {
          date: props.account.balanceDate
            ? dayjs(props.account.balanceDate).format(`${dateFormat} LT`)
            : t("never"),
        })}
      </DimmedText>
    </Stack>
  );
};

export default AccountItem;
