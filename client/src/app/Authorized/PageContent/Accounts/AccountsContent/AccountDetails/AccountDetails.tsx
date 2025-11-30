import { Accordion, Button, Group, Skeleton, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { IAccountResponse } from "~/models/account";
import { IBalanceResponse } from "~/models/balance";
import BalanceItems from "./BalanceItems/BalanceItems";
import AddBalance from "./AddBalance/AddBalance";
import Drawer from "~/components/Drawer/Drawer";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import SurfaceAccordionRoot from "~/components/Accordion/Surface/SurfaceAccordionRoot/SurfaceAccordionRoot";

interface AccountDetailsProps {
  isOpen: boolean;
  close: () => void;
  account: IAccountResponse | undefined;
  currency: string;
}

const AccountDetails = (props: AccountDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

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
      dayjs().subtract(chartLookbackMonths, "months")
    )
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<PrimaryText size="lg">Account Details</PrimaryText>}
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
    >
      {!props.account ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <DimmedText size="xs">Account Name</DimmedText>
              <PrimaryText size="lg">{props.account.name}</PrimaryText>
            </Stack>
            <Stack gap={0}>
              <DimmedText size="xs">Account Type</DimmedText>
              <PrimaryText size="lg">
                {props.account.subtype || props.account.type}
              </PrimaryText>
            </Stack>
          </Group>
          <SurfaceAccordionRoot
            variant="separated"
            defaultValue={["add-balance", "chart", "balances"]}
            multiple
          >
            <Accordion.Item value="add-balance">
              <Accordion.Control>
                <PrimaryText>Add Balance</PrimaryText>
              </Accordion.Control>
              <Accordion.Panel>
                <AddBalance
                  accountId={props.account.id}
                  currency={props.currency}
                />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item value="chart">
              <Accordion.Control>
                <PrimaryText>Account Trends</PrimaryText>
              </Accordion.Control>
              <Accordion.Panel>
                <Group>
                  <Button
                    variant={chartLookbackMonths === 3 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(3)}
                  >
                    3 months
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 6 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(6)}
                  >
                    6 months
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 12 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(12)}
                  >
                    12 months
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
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item value="balances">
              <Accordion.Control>
                <PrimaryText>Recent Balances</PrimaryText>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  {balancesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedBalances.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">No balance entries.</DimmedText>
                    </Group>
                  ) : (
                    <BalanceItems
                      balances={sortedBalances}
                      currency={props.currency}
                    />
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item value="deleted-balances">
              <Accordion.Control>
                <PrimaryText>Deleted Balances</PrimaryText>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  {balancesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedDeletedBalances.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">
                        No deleted balance entries.
                      </DimmedText>
                    </Group>
                  ) : (
                    <BalanceItems
                      balances={sortedDeletedBalances}
                      currency={props.currency}
                    />
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </SurfaceAccordionRoot>
        </Stack>
      )}
    </Drawer>
  );
};

export default AccountDetails;
