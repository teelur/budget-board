import {
  Accordion as MantineAccordion,
  Button,
  Group,
  Skeleton,
  Stack,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { IAccountResponse } from "~/models/account";
import { IBalanceResponse } from "~/models/balance";
import BalanceItems from "./BalanceItems/BalanceItems";
import AddBalance from "./AddBalance/AddBalance";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface AccountDetailsProps {
  isOpen: boolean;
  close: () => void;
  account: IAccountResponse | undefined;
  currency: string;
}

const AccountDetails = (props: AccountDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { request } = useAuth();

  const balancesQuery = useQuery({
    queryKey: ["balances", props.account?.id],
    queryFn: async (): Promise<IBalanceResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/balance",
        method: "GET",
        params: { accountId: props.account?.id },
      });

      if (res.status === 200) {
        return res.data as IBalanceResponse[];
      }

      return [];
    },
    enabled: !!props.account?.id && props.isOpen,
  });

  const sortedBalances =
    balancesQuery.data
      ?.filter((balance) => !balance.deleted)
      .sort((a, b) => dayjs(b.dateTime).diff(dayjs(a.dateTime))) ?? [];

  const sortedDeletedBalances =
    balancesQuery.data
      ?.filter((balance) => balance.deleted)
      .sort((a, b) => dayjs(b.dateTime).diff(dayjs(a.dateTime))) ?? [];

  const balancesForChart = sortedBalances.filter((balance) =>
    dayjs(balance.dateTime).isAfter(
      dayjs().subtract(chartLookbackMonths, "months"),
    ),
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<PrimaryText size="lg">{t("account_details")}</PrimaryText>}
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
                {props.account.subtype || props.account.type}
              </PrimaryText>
            </Stack>
          </Group>
          <Accordion
            defaultValue={["add-balance", "chart", "balances"]}
            elevation={1}
          >
            <MantineAccordion.Item value="add-balance">
              <MantineAccordion.Control>
                <PrimaryText>{t("add_balance")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <AddBalance
                  accountId={props.account.id}
                  currency={props.currency}
                />
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="chart">
              <MantineAccordion.Control>
                <PrimaryText>{t("account_trends")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
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
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="balances">
              <MantineAccordion.Control>
                <PrimaryText>{t("recent_balances")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {balancesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedBalances.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">
                        {t("no_balance_entries")}
                      </DimmedText>
                    </Group>
                  ) : (
                    <BalanceItems
                      balances={sortedBalances}
                      currency={props.currency}
                    />
                  )}
                </Stack>
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="deleted-balances">
              <MantineAccordion.Control>
                <PrimaryText>{t("deleted_balances")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {balancesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedDeletedBalances.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">
                        {t("no_deleted_balance_entries")}
                      </DimmedText>
                    </Group>
                  ) : (
                    <BalanceItems
                      balances={sortedDeletedBalances}
                      currency={props.currency}
                    />
                  )}
                </Stack>
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default AccountDetails;
