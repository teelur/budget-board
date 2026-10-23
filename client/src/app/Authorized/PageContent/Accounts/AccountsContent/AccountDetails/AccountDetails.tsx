import { Group, Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  CollapsibleCard,
  Drawer,
  HeadingText,
  Skeleton,
} from "@teelur/budget-board-ui";
import React from "react";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { IAccountResponse } from "~/models/account";
import BalanceItems from "./BalanceItems/BalanceItems";
import AddBalance from "./AddBalance/AddBalance";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import {
  getIsParentAccountType,
  getParentAccountType,
} from "~/helpers/accountType";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useBalancesQuery } from "~/hooks/queries/useBalancesQuery";

interface AccountDetailsProps {
  isOpen: boolean;
  close: () => void;
  account: IAccountResponse;
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
      title={t("account_details")}
    >
      {!props.account ? (
        <Skeleton height={425} />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <BodyText tone="secondary" size="xs">
                {t("account_name")}
              </BodyText>
              <BodyText tone="primary" size="lg" fw={500}>
                {props.account.name}
              </BodyText>
            </Stack>
            <Stack gap={0}>
              <BodyText tone="secondary" size="xs">
                {t("account_type")}
              </BodyText>
              <BodyText tone="primary" size="lg" fw={500}>
                {props.account.type ? props.account.type : t("no_type")}
              </BodyText>
              {!getIsParentAccountType(props.account.type, allAccountTypes) && (
                <BodyText tone="secondary" size="xs">
                  {getParentAccountType(props.account.type, allAccountTypes)}
                </BodyText>
              )}
            </Stack>
          </Group>
          <Stack gap="0.5rem">
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("add_balance")}
                </HeadingText>
              }
            >
              <AddBalance accountId={props.account.id} />
            </CollapsibleCard>
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("account_trends")}
                </HeadingText>
              }
            >
              <Group gap="0.5rem">
                <Button
                  variant="filled"
                  color="primary"
                  selected={chartLookbackMonths === 3}
                  size="compact-xs"
                  onClick={() => setChartLookbackMonths(3)}
                >
                  {t("3_months")}
                </Button>
                <Button
                  variant="filled"
                  color="primary"
                  selected={chartLookbackMonths === 6}
                  size="compact-xs"
                  onClick={() => setChartLookbackMonths(6)}
                >
                  {t("6_months")}
                </Button>
                <Button
                  variant="filled"
                  color="primary"
                  selected={chartLookbackMonths === 12}
                  size="compact-xs"
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
            </CollapsibleCard>
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("recent_balances")}
                </HeadingText>
              }
            >
              <Stack gap="0.5rem">
                {balancesQuery.isPending && <Skeleton height={20} />}
                {sortedBalances.length === 0 ? (
                  <Group justify="center">
                    <BodyText tone="muted" size="sm">
                      {t("no_balance_entries")}
                    </BodyText>
                  </Group>
                ) : (
                  <BalanceItems balances={sortedBalances} />
                )}
              </Stack>
            </CollapsibleCard>
          </Stack>
        </Stack>
      )}
    </Drawer>
  );
};

export default AccountDetails;
