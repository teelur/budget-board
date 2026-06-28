import { Button, Group, Skeleton, Stack } from "@mantine/core";
import { IGoalResponse } from "~/models/goal";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { IAccountResponse } from "~/models/account";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { DatesRangeValue } from "@mantine/dates";
import { IBalanceResponse } from "~/models/balance";
import Drawer from "~/components/core/Drawer/Drawer";
import Card from "~/components/core/Card/Card";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useBalancesQuery } from "~/hooks/queries/useBalancesQuery";

interface GoalDetailsProps {
  goal: IGoalResponse | null;
  isOpen: boolean;
  doClose: () => void;
}

const GoalDetails = (props: GoalDetailsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const balancesQuery = useBalancesQuery({
    accountIds: props.goal?.accounts.map((account) => account.id) ?? [],
    enabled: props.isOpen && props.goal !== null,
  });

  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(3);

  const dateRange: DatesRangeValue<string> = [
    dayjs().subtract(chartLookbackMonths, "month").toISOString(),
    dayjs().toISOString(),
  ];

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.doClose}
      position="right"
      size="md"
      title={
        <PrimaryHeading component="span" order={4}>
          {t("goal_details")}
        </PrimaryHeading>
      }
    >
      {props.goal === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Accordion elevation={1}>
          <Accordion.Item
            title={<PrimaryHeading order={5}>{t("accounts")}</PrimaryHeading>}
          >
            <Stack gap="0.5rem">
              {props.goal.accounts.map((account: IAccountResponse) => (
                <Card key={account.id} elevation={2}>
                  <AccountItem account={account} />
                </Card>
              ))}
            </Stack>
          </Accordion.Item>
          <Accordion.Item
            title={
              <PrimaryHeading order={5}>{t("goal_trends")}</PrimaryHeading>
            }
          >
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
          </Accordion.Item>
        </Accordion>
      )}
    </Drawer>
  );
};

export default GoalDetails;
