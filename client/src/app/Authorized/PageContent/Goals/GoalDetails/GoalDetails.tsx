import {
  Accordion as MantineAccordion,
  Button,
  Group,
  Skeleton,
  Stack,
} from "@mantine/core";
import { IGoalResponse } from "~/models/goal";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { IAccountResponse } from "~/models/account";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { DatesRangeValue } from "@mantine/dates";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueries } from "@tanstack/react-query";
import { IBalanceResponse } from "~/models/balance";
import { AxiosResponse } from "axios";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Card from "~/components/core/Card/Card";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface GoalDetailsProps {
  goal: IGoalResponse | null;
  isOpen: boolean;
  doClose: () => void;
}

const GoalDetails = (props: GoalDetailsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useDate();
  const { request } = useAuth();

  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(3);

  const dateRange: DatesRangeValue<string> = [
    dayjs().subtract(chartLookbackMonths, "month").toISOString(),
    dayjs().toISOString(),
  ];

  const balancesQuery = useQueries({
    queries: (props.goal?.accounts ?? []).map((account: IAccountResponse) => ({
      queryKey: ["balances", account.id],
      queryFn: async (): Promise<IBalanceResponse[]> => {
        const res: AxiosResponse = await request({
          url: "/api/balance",
          method: "GET",
          params: { accountId: account.id },
        });

        if (res.status === 200) {
          return res.data as IBalanceResponse[];
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
      title={<PrimaryText size="lg">{t("goal_details")}</PrimaryText>}
    >
      {props.goal === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Accordion defaultValue={["accounts", "chart"]} elevation={1}>
          <MantineAccordion.Item value="accounts">
            <MantineAccordion.Control>
              <PrimaryText size="md">{t("accounts")}</PrimaryText>
            </MantineAccordion.Control>
            <MantineAccordion.Panel>
              <Stack gap="0.5rem">
                {props.goal.accounts.map((account: IAccountResponse) => (
                  <Card key={account.id} elevation={2}>
                    <AccountItem account={account} />
                  </Card>
                ))}
              </Stack>
            </MantineAccordion.Panel>
          </MantineAccordion.Item>
          <MantineAccordion.Item value="chart">
            <MantineAccordion.Control>
              <PrimaryText size="md">{t("goal_trends")}</PrimaryText>
            </MantineAccordion.Control>
            <MantineAccordion.Panel>
              <Stack>
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
                  items={props.goal.accounts}
                  values={
                    balancesQuery.data?.map((balance: IBalanceResponse) => ({
                      ...balance,
                      parentId: balance.accountID || "",
                    })) ?? []
                  }
                  dateRange={dateRange}
                  invertYAxis={props.goal.amount === 0}
                />
              </Stack>
            </MantineAccordion.Panel>
          </MantineAccordion.Item>
        </Accordion>
      )}
    </Drawer>
  );
};

export default GoalDetails;
