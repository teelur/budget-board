import {
  Accordion,
  Button,
  Card,
  Drawer,
  Group,
  Skeleton,
  Stack,
  Text,
} from "@mantine/core";
import { IGoalResponse } from "~/models/goal";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { IAccount } from "~/models/account";
import BalanceChart from "~/components/Charts/BalanceChart/BalanceChart";
import { DatesRangeValue } from "@mantine/dates";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueries } from "@tanstack/react-query";
import { IBalance } from "~/models/balance";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";

interface GoalDetailsProps {
  goal: IGoalResponse | null;
  isOpen: boolean;
  doClose: () => void;
}

const GoalDetails = (props: GoalDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(3);

  const dateRange: DatesRangeValue<string> = [
    dayjs().subtract(chartLookbackMonths, "month").toISOString(),
    dayjs().toISOString(),
  ];

  const { request } = React.useContext<any>(AuthContext);
  const balancesQuery = useQueries({
    queries: (props.goal?.accounts ?? []).map((account: IAccount) => ({
      queryKey: ["balances", account.id],
      queryFn: async (): Promise<IBalance[]> => {
        const res: AxiosResponse = await request({
          url: "/api/balance",
          method: "GET",
          params: { accountId: account.id },
        });

        if (res.status === 200) {
          return res.data as IBalance[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.doClose}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Goal Details
        </Text>
      }
    >
      {props.goal === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Accordion
          variant="separated"
          defaultValue={["accounts", "chart"]}
          multiple
        >
          <Accordion.Item
            value="accounts"
            bg="var(--mantine-color-content-background)"
          >
            <Accordion.Control>
              <Text fw={600}>Accounts</Text>
            </Accordion.Control>
            <Accordion.Panel>
              <Stack gap="0.5rem">
                {props.goal.accounts.map((account: IAccount) => (
                  <Card key={account.id} radius="md">
                    <AccountItem account={account} />
                  </Card>
                ))}
              </Stack>
            </Accordion.Panel>
          </Accordion.Item>
          <Accordion.Item
            value="chart"
            bg="var(--mantine-color-content-background)"
          >
            <Accordion.Control>
              <Text fw={600}>Goal Trends</Text>
            </Accordion.Control>
            <Accordion.Panel>
              <Stack>
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
                  accounts={props.goal.accounts}
                  balances={balancesQuery.data ?? []}
                  dateRange={dateRange}
                  invertYAxis={props.goal.amount === 0}
                />
              </Stack>
            </Accordion.Panel>
          </Accordion.Item>
        </Accordion>
      )}
    </Drawer>
  );
};

export default GoalDetails;
