import classes from "./AccountItem.module.css";

import { Group, Stack } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import React from "react";
import PrimaryText from "../core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "../core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "../core/Text/StatusText/StatusText";
import DimmedText from "../core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat } = useLocale();

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
        <StatusText
          className={classes.amount}
          amount={props.account.currentBalance}
        >
          <SensitiveAmount amount={props.account.currentBalance} />
        </StatusText>
      </Stack>
    </Group>
  );
};

export default AccountItem;
