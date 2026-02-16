import { Flex, Group } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { Trans } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface CompletedGoalCardProps {
  goal: IGoalResponse;
}

const CompletedGoalCard = (props: CompletedGoalCardProps): React.ReactNode => {
  const { request } = useAuth();
  const { dayjs } = useDate();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  return (
    <Card elevation={2}>
      <Flex
        justify="space-between"
        align={{ base: "start", xs: "center" }}
        direction={{ base: "column", xs: "row" }}
      >
        <PrimaryText size="lg">{props.goal.name}</PrimaryText>
        <Flex
          gap={0}
          direction={{ base: "row", xs: "column" }}
          justify={{ base: "space-between", xs: "flex-end" }}
          w={{ base: "100%", xs: "auto" }}
        >
          <Group gap="0.25rem">
            <Trans
              i18nKey="goal_completed_total_styled"
              values={{
                amount: convertNumberToCurrency(
                  getGoalTargetAmount(
                    props.goal.amount,
                    props.goal.initialAmount,
                  ),
                  true,
                  userSettingsQuery.data?.currency ?? "USD",
                ),
              }}
              components={[
                <DimmedText size="sm" key="label" />,
                <PrimaryText size="md" key="amount" />,
              ]}
            />
          </Group>
          <Group gap="0.25rem">
            <Trans
              i18nKey="goal_completed_date_styled"
              values={{
                date: dayjs(props.goal.completed).format("MMMM YYYY"),
              }}
              components={[
                <DimmedText size="sm" key="label" />,
                <PrimaryText size="md" key="amount" />,
              ]}
            />
          </Group>
        </Flex>
      </Flex>
    </Card>
  );
};

export default CompletedGoalCard;
