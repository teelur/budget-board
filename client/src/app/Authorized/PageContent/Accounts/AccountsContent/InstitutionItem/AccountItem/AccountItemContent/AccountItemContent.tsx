import { ActionIcon, Badge, Group, Stack } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { AccountSource, IAccountResponse } from "~/models/account";

interface IAccountItemContentProps {
  account: IAccountResponse;
  userCurrency: string;
  toggle: () => void;
}

const AccountItemContent = (props: IAccountItemContentProps) => {
  const { t } = useTranslation();

  const getAccountSourceBadgeColor = (): string => {
    switch (props.account.source) {
      case AccountSource.SimpleFIN:
        return "blue";
      case AccountSource.Manual:
      default:
        return "gray";
    }
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
            variant="transparent"
            size="md"
            onClick={(e) => {
              e.stopPropagation();
              props.toggle();
            }}
          >
            <PencilIcon size={16} />
          </ActionIcon>
          <Badge>
            {t("interest_rate_message", {
              rate: ((props.account.interestRate ?? 0) * 100).toFixed(2),
            })}
          </Badge>
          {props.account.hideAccount && (
            <Badge bg="var(--button-color-warning)">{t("hidden")}</Badge>
          )}
          {props.account.hideTransactions && (
            <Badge bg="purple">{t("hidden_transactions")}</Badge>
          )}
          <Badge bg={getAccountSourceBadgeColor()}>
            {t(props.account.source)}
          </Badge>
        </Group>
        <StatusText amount={props.account.currentBalance} size="md">
          {convertNumberToCurrency(
            props.account.currentBalance,
            true,
            props.userCurrency
          )}
        </StatusText>
      </Group>
      <Group justify="space-between" align="center">
        <DimmedText size="sm">
          {props.account.subtype ? props.account.subtype : props.account.type}
        </DimmedText>
        <DimmedText size="sm">
          {t("last_updated", {
            date: dayjs(props.account.balanceDate).isValid()
              ? dayjs(props.account.balanceDate).format("L LT")
              : t("never"),
          })}
        </DimmedText>
      </Group>
    </Stack>
  );
};

export default AccountItemContent;
