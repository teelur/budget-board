import { Group, Stack } from "@mantine/core";
import { ActionIcon, Badge, AmountText } from "@teelur/budget-board-ui";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  getIsParentAccountType,
  getParentAccountType,
} from "~/helpers/accountType";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { AccountSource, IAccountResponse } from "~/models/account";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface IAccountItemContentProps {
  account: IAccountResponse;
  toggle: () => void;
}

const AccountItemContent = (props: IAccountItemContentProps) => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { allAccountTypes } = useAccountTypes();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  const getAccountSourceBadgeColor = (): "info" | "success" | "neutral" => {
    switch (props.account.source) {
      case AccountSource.SimpleFIN:
        return "info";
      case AccountSource.LunchFlow:
        return "success";
      case AccountSource.Manual:
      default:
        return "neutral";
    }
  };

  const getAccountTypeDisplay = (): React.ReactNode => {
    if (props.account.type?.length === 0) {
      return <DimmedText size="sm">{t("no_type")}</DimmedText>;
    }

    const isParentAccountType = getIsParentAccountType(
      props.account.type,
      allAccountTypes,
    );

    const accountType = isParentAccountType
      ? props.account.type
      : getParentAccountType(props.account.type, allAccountTypes);

    return (
      <Group gap="0.25rem">
        <DimmedText size="sm">{accountType}</DimmedText>
        {!isParentAccountType && (
          <>
            <ChevronRightIcon size={14} />
            <DimmedText size="sm">{props.account.type}</DimmedText>
          </>
        )}
      </Group>
    );
  };

  return (
    <Stack gap={0} flex="1 1 auto">
      <Group justify="space-between" align="center">
        <Group gap="0.5rem" align="center">
          <PrimaryText size="md">
            {props.account.name && props.account.name.length > 0
              ? props.account.name
              : t("no_name")}
          </PrimaryText>
          <ActionIcon
            variant="ghost"
            color="primary"
            size="compact-xs"
            onClick={(e) => {
              e.stopPropagation();
              props.toggle();
            }}
          >
            <PencilIcon size={16} />
          </ActionIcon>
          <Badge variant="filled" color="primary" size="xs">
            {t("interest_rate_message", {
              rate: new Intl.NumberFormat(intlLocale, {
                style: "percent",
                maximumFractionDigits: 2,
              }).format(props.account.interestRate ?? 0),
            })}
          </Badge>
          {props.account.hideAccount && (
            <Badge variant="filled" color="secondary" size="xs">
              {t("hidden")}
            </Badge>
          )}
          {props.account.hideTransactions && (
            <Badge variant="filled" color="accent" size="xs">
              {t("hidden_transactions")}
            </Badge>
          )}
          <Badge
            variant="filled"
            color={getAccountSourceBadgeColor()}
            size="xs"
          >
            {t(props.account.source)}
          </Badge>
        </Group>
        <AmountText
          amount={props.account.currentBalance}
          size="md"
          isSensitive={isPrivacyModeEnabled}
        >
          <SensitiveAmount amount={props.account.currentBalance} />
        </AmountText>
      </Group>
      <Group justify="space-between" align="center">
        {getAccountTypeDisplay()}
        <DimmedText size="sm">
          {t("last_updated", {
            date: dayjs(props.account.balanceDate).isValid()
              ? dayjs(props.account.balanceDate).format(`${dateFormat}`)
              : t("never"),
          })}
        </DimmedText>
      </Group>
    </Stack>
  );
};

export default AccountItemContent;
