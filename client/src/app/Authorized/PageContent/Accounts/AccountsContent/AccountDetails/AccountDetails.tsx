import {
  Accordion,
  Button,
  Drawer,
  Group,
  Skeleton,
  Stack,
  Text,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import BalanceChart from "~/components/Charts/BalanceChart/BalanceChart";
import { IAccountResponse } from "~/models/account";
import { IBalance } from "~/models/balance";
import BalanceItems from "./BalanceItems/BalanceItems";
import AddBalance from "./AddBalance/AddBalance";

interface AccountDetailsProps {
  isOpen: boolean;
  close: () => void;
  account: IAccountResponse | undefined;
  currency: string;
}

const AccountDetails = (props: AccountDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { request } = React.useContext<any>(AuthContext);
  const balancesQuery = useQuery({
    queryKey: ["balances", props.account?.id],
    queryFn: async (): Promise<IBalance[]> => {
      const res: AxiosResponse = await request({
        url: "/api/balance",
        method: "GET",
        params: { accountId: props.account?.id },
      });

      if (res.status === 200) {
        return res.data as IBalance[];
      }

      return [];
    },
  });

  const sortedBalances =
    balancesQuery.data?.sort((a, b) =>
      dayjs(b.dateTime).diff(dayjs(a.dateTime))
    ) ?? [];

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
      title={
        <Text size="lg" fw={600}>
          Account Details
        </Text>
      }
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
              <Text size="xs" fw={500} c="dimmed">
                Account Name
              </Text>
              <Text size="lg" fw={600}>
                {props.account.name}
              </Text>
            </Stack>
            <Stack gap={0}>
              <Text size="xs" fw={500} c="dimmed">
                Account Type
              </Text>
              <Text size="lg" fw={600}>
                {props.account.subtype || props.account.type}
              </Text>
            </Stack>
          </Group>
          <Accordion
            variant="separated"
            defaultValue={["chart", "balances"]}
            multiple
          >
            <Accordion.Item
              value="add-balance"
              bg="var(--mantine-color-content-background)"
            >
              <Accordion.Control>
                <Text>Add Balance</Text>
              </Accordion.Control>
              <Accordion.Panel>
                <AddBalance
                  accountId={props.account.id}
                  currency={props.currency}
                />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item
              value="chart"
              bg="var(--mantine-color-content-background)"
            >
              <Accordion.Control>
                <Text>Account Trends</Text>
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
                <BalanceChart
                  accounts={[props.account]}
                  balances={balancesForChart}
                  dateRange={[
                    dayjs().subtract(chartLookbackMonths, "months").toString(),
                    dayjs().toString(),
                  ]}
                />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item
              value="balances"
              bg="var(--mantine-color-content-background)"
            >
              <Accordion.Control>Recent Balances</Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  {balancesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {balancesQuery.data && balancesQuery.data.length === 0 ? (
                    <Text size="sm" c="dimmed">
                      No balance entries
                    </Text>
                  ) : (
                    <BalanceItems
                      balances={sortedBalances}
                      currency={props.currency}
                    />
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default AccountDetails;
