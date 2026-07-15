import classes from "./AccountItem.module.css";

import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Group, Stack } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import React from "react";
import PrimaryText from "../core/Text/PrimaryText/PrimaryText";
import StatusText from "../core/Text/StatusText/StatusText";
import DimmedText from "../core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface AccountItemProps {
  account: IAccountResponse;
  onClick?: () => void;
}

const AccountItem = (props: AccountItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

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
          {convertNumberToCurrency(
            props.account.currentBalance,
            true,
            preferredCurrency,
            SignDisplay.Auto,
            intlLocale,
          )}
        </StatusText>
      </Stack>
    </Group>
  );
};

export default AccountItem;
