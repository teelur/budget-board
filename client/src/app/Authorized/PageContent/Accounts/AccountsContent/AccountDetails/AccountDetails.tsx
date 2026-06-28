import { Button, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { IAccountResponse } from "~/models/account";
import BalanceItems from "./BalanceItems/BalanceItems";
import AddBalance from "./AddBalance/AddBalance";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import {
  getIsParentAccountType,
  getParentAccountType,
} from "~/helpers/accountType";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useBalancesQuery } from "~/hooks/queries/useBalancesQuery";

interface AccountDetailsProps {
  isOpen: boolean;
  close: () => void;
  account: IAccountResponse;
  currency: string;
}

const AccountDetails = (props: AccountDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { allAccountTypes } = useAccountTypes();
  const balancesQuery = useBalancesQuery({
    accountIds: props.account.id ? [props.account.id] : [],
    enabled: props.isOpen,
  });

  const sortedBalances =
    balancesQuery.data?.sort((a, b) => dayjs(b.date).diff(dayjs(a.date))) ?? [];

  const balancesForChart = sortedBalances.filter((balance) =>
    dayjs(balance.date).isAfter(
      dayjs().subtract(chartLookbackMonths, "months"),
    ),
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={
        <PrimaryHeading component="span" order={4}>
          {t("account_details")}
        </PrimaryHeading>
      }
    >
      {!props.account ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <DimmedText size="xs">{t("account_name")}</DimmedText>
              <PrimaryText size="lg">{props.account.name}</PrimaryText>
            </Stack>
            <Stack gap={0}>
              <DimmedText size="xs">{t("account_type")}</DimmedText>
              <PrimaryText size="lg">
                {props.account.type ? props.account.type : t("no_type")}
              </PrimaryText>
              {!getIsParentAccountType(props.account.type, allAccountTypes) && (
                <DimmedText size="xs">
                  {getParentAccountType(props.account.type, allAccountTypes)}
                </DimmedText>
              )}
            </Stack>
          </Group>
          <Accordion elevation={1}>
            <Accordion.Item
              title={
                <PrimaryHeading order={5}>{t("add_balance")}</PrimaryHeading>
              }
            >
              <AddBalance
                accountId={props.account.id}
                currency={props.currency}
              />
            </Accordion.Item>
            <Accordion.Item
              title={
                <PrimaryHeading order={5}>{t("account_trends")}</PrimaryHeading>
              }
            >
              <Group>
                <Button
                  variant={chartLookbackMonths === 3 ? "filled" : "outline"}
                  size="xs"
                  onClick={() => setChartLookbackMonths(3)}
                >
                  {t("3_months")}
                </Button>
                <Button
                  variant={chartLookbackMonths === 6 ? "filled" : "outline"}
                  size="xs"
                  onClick={() => setChartLookbackMonths(6)}
                >
                  {t("6_months")}
                </Button>
                <Button
                  variant={chartLookbackMonths === 12 ? "filled" : "outline"}
                  size="xs"
                  onClick={() => setChartLookbackMonths(12)}
                >
                  {t("12_months")}
                </Button>
              </Group>
              <ValueChart
                items={[
                  {
                    id: props.account.id,
                    name: props.account.name,
                  },
                ]}
                values={balancesForChart.map((balance) => ({
                  ...balance,
                  parentId: balance.accountID || "",
                }))}
                dateRange={[
                  dayjs().subtract(chartLookbackMonths, "months").toString(),
                  dayjs().toString(),
                ]}
              />
            </Accordion.Item>
            <Accordion.Item
              title={
                <PrimaryHeading order={5}>
                  {t("recent_balances")}
                </PrimaryHeading>
              }
            >
              <Stack gap="0.5rem">
                {balancesQuery.isPending && (
                  <Skeleton height={20} radius="lg" />
                )}
                {sortedBalances.length === 0 ? (
                  <Group justify="center">
                    <DimmedText size="sm">{t("no_balance_entries")}</DimmedText>
                  </Group>
                ) : (
                  <BalanceItems
                    balances={sortedBalances}
                    currency={props.currency}
                  />
                )}
              </Stack>
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default AccountDetails;
