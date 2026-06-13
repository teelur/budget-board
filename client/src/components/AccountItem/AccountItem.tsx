import classes from "./AccountItem.module.css";

import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Group, Skeleton, Stack } from "@mantine/core";
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
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { userSettingsQueryKey } from "~/helpers/requests";


interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const { dayjs, dateFormat, intlLocale } = useLocale();

  const userSettingsQuery = useQuery({
    queryKey: [userSettingsQueryKey],
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
    <Group
      h="100%"
      justify="space-between"
      wrap="nowrap"
      className={props.onClick ? classes.root : undefined}
      onClick={props.onClick}
    >
      <Stack h="100%" gap={0}>
        <PrimaryText className={classes.title}>
          {props.account.name}
        </PrimaryText>
        <DimmedText className={classes.timestamp} size="xs">
          {t("last_updated", {
            date: dayjs(props.account.balanceDate).isValid()
              ? dayjs(props.account.balanceDate).format(`${dateFormat}`)
              : t("never"),
          })}
        </DimmedText>
      </Stack>
      <Stack h="100%" justify="flex-start">
        {userSettingsQuery.isPending ? (
          <Skeleton height={25} width={100} radius="md" />
        ) : (
          <StatusText
            className={classes.amount}
            amount={props.account.currentBalance}
          >
            {convertNumberToCurrency(
              props.account.currentBalance,
              true,
              userSettingsQuery.data?.currency ?? "USD",
              SignDisplay.Auto,
              intlLocale,
            )}
          </StatusText>
        )}
      </Stack>
    </Group>
  );
};

export default AccountItem;