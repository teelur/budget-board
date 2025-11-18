import classes from "./GoalCardContent.module.css";

import {
  ActionIcon,
  Badge,
  Flex,
  Group,
  Progress,
  Stack,
  Text,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";
import { PencilIcon } from "lucide-react";

interface GoalCardContentProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  toggleIsSelected: () => void;
}

const GoalCardContent = (props: GoalCardContentProps): React.ReactNode => {
  const { request } = useAuth();

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
    <Group wrap="nowrap" className={classes.root}>
      <Stack w="100%" gap="0.1rem">
        <Flex className={classes.header}>
          <Group align="center" gap={10} wrap="nowrap">
            <Text size="lg" fw={600}>
              {props.goal.name}
            </Text>
            {props.includeInterest && props.goal.interestRate && (
              <Badge variant="light" flex="0 0 auto">
                {props.goal.interestRate.toLocaleString(undefined, {
                  style: "percent",
                  minimumFractionDigits: 2,
                })}{" "}
                APR
              </Badge>
            )}
            <ActionIcon
              variant="transparent"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                props.toggleIsSelected();
              }}
            >
              <PencilIcon size={16} />
            </ActionIcon>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            {userSettingsQuery.isPending ? null : (
              <Text size="lg" fw={600}>
                {convertNumberToCurrency(
                  sumAccountsTotalBalance(props.goal.accounts) -
                    props.goal.initialAmount,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
            <Text size="md" fw={600}>
              of
            </Text>
            <Text size="lg" fw={600}>
              {convertNumberToCurrency(
                getGoalTargetAmount(
                  props.goal.amount,
                  props.goal.initialAmount
                ),
                false,
                userSettingsQuery.data?.currency ?? "USD"
              )}
            </Text>
          </Flex>
        </Flex>
        <Progress.Root size={18} radius="xl">
          <Progress.Section value={props.goal.percentComplete}>
            <Progress.Label>
              {props.goal.percentComplete.toFixed(0)}%
            </Progress.Label>
          </Progress.Section>
        </Progress.Root>
        <Flex className={classes.footer}>
          <Group align="center" gap="sm">
            <Flex align="center" gap="0.25rem">
              <Text size="sm" fw={600} c="dimmed">
                {"Projected: "}
              </Text>
              <Text size="sm" fw={600} c="dimmed">
                {new Date(props.goal.completeDate).toLocaleDateString("en-US", {
                  year: "numeric",
                  month: "long",
                })}
              </Text>
            </Flex>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            {userSettingsQuery.isPending ? null : (
              <Text
                c={
                  props.goal.monthlyContributionProgress <
                  props.goal.monthlyContribution
                    ? "red"
                    : "green"
                }
                size="md"
                fw={600}
              >
                {convertNumberToCurrency(
                  props.goal.monthlyContributionProgress,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
            <Text size="sm" fw={600}>
              of
            </Text>
            {userSettingsQuery.isPending ? null : (
              <Text size="md" fw={600}>
                {convertNumberToCurrency(
                  props.goal.monthlyContribution,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
            <Text size="sm" fw={600}>
              this month
            </Text>
          </Flex>
        </Flex>
      </Stack>
    </Group>
  );
};

export default GoalCardContent;
