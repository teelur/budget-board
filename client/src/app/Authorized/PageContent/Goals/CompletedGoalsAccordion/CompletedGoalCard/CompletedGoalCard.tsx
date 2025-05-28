import { Card, Flex, Group, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";

interface CompletedGoalCardProps {
  goal: IGoalResponse;
}

const CompletedGoalCard = (props: CompletedGoalCardProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
    <Card padding="0.5rem" radius="md">
      <Flex
        justify="space-between"
        align={{ base: "start", xs: "center" }}
        direction={{ base: "column", xs: "row" }}
      >
        <Text size="lg" fw={600}>
          {props.goal.name}
        </Text>
        <Flex
          gap={0}
          direction={{ base: "row", xs: "column" }}
          justify={{ base: "space-between", xs: "flex-end" }}
          w={{ base: "100%", xs: "auto" }}
        >
          <Group gap={5}>
            <Text>Total:</Text>
            {userSettingsQuery.isPending ? null : (
              <Text fw={600}>
                {convertNumberToCurrency(
                  getGoalTargetAmount(
                    props.goal.amount,
                    props.goal.initialAmount
                  ),
                  true,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
          </Group>
          <Group gap={5}>
            <Text>Completed:</Text>
            <Text fw={600}>
              {new Date(props.goal.completed ?? 0).toLocaleDateString()}
            </Text>
          </Group>
        </Flex>
      </Flex>
    </Card>
  );
};

export default CompletedGoalCard;
