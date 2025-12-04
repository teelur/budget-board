import classes from "./GoalCardContent.module.css";

import { ActionIcon, Badge, Flex, Group, Stack } from "@mantine/core";
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
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { StatusColorType } from "~/helpers/budgets";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import Progress from "~/components/core/Progress/Progress";

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
    <Group style={{ containerType: "inline-size" }} wrap="nowrap">
      <Stack w="100%" gap="0.1rem">
        <Flex className={classes.header}>
          <Group align="center" gap={10} wrap="nowrap">
            <PrimaryText size="lg">{props.goal.name}</PrimaryText>
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
              size="md"
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
              <PrimaryText size="lg">
                {convertNumberToCurrency(
                  sumAccountsTotalBalance(props.goal.accounts) -
                    props.goal.initialAmount,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </PrimaryText>
            )}
            <DimmedText size="md">of</DimmedText>
            <PrimaryText size="lg">
              {convertNumberToCurrency(
                getGoalTargetAmount(
                  props.goal.amount,
                  props.goal.initialAmount
                ),
                false,
                userSettingsQuery.data?.currency ?? "USD"
              )}
            </PrimaryText>
          </Flex>
        </Flex>
        <Progress
          size={18}
          percentComplete={props.goal.percentComplete}
          amount={0}
          limit={0}
          type={ProgressType.Default}
          elevation={1}
        />
        <Flex className={classes.footer}>
          <Group align="center" gap="sm">
            <Flex align="center" gap="0.25rem">
              <DimmedText size="sm">{"Projected: "}</DimmedText>
              <PrimaryText size="sm">
                {new Date(props.goal.completeDate).toLocaleDateString("en-US", {
                  year: "numeric",
                  month: "long",
                })}
              </PrimaryText>
            </Flex>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            {userSettingsQuery.isPending ? null : (
              <StatusText
                amount={props.goal.monthlyContributionProgress}
                total={props.goal.monthlyContribution}
                type={StatusColorType.Target}
                size="md"
              >
                {convertNumberToCurrency(
                  props.goal.monthlyContributionProgress,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </StatusText>
            )}
            <DimmedText size="sm">of</DimmedText>
            {userSettingsQuery.isPending ? null : (
              <PrimaryText size="md">
                {convertNumberToCurrency(
                  props.goal.monthlyContribution,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </PrimaryText>
            )}
            <DimmedText size="sm">this month</DimmedText>
          </Flex>
        </Flex>
      </Stack>
    </Group>
  );
};

export default GoalCardContent;
