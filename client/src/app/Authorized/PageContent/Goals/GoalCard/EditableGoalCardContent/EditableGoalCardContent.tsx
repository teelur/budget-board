import classes from "./EditableGoalCardContent.module.css";

import {
  ActionIcon,
  Badge,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import { IGoalResponse, IGoalUpdateRequest } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { PencilIcon, TrashIcon } from "lucide-react";
import { useField } from "@mantine/form";
import { DateValue } from "@mantine/dates";
import dayjs from "dayjs";
import { getGoalTargetAmount } from "~/helpers/goals";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { StatusColorType } from "~/helpers/budgets";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";

interface GoalCardContentProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  toggleIsSelected: () => void;
}

const EditableGoalCardContent = (
  props: GoalCardContentProps
): React.ReactNode => {
  const { request } = useAuth();

  const goalNameField = useField<string>({
    initialValue: props.goal.name,
  });
  const goalTargetAmountField = useField<number>({
    initialValue: props.goal.amount,
  });
  const goalMonthlyContributionField = useField<number>({
    initialValue: props.goal.monthlyContribution,
  });
  const goalTargetDateField = useField<DateValue>({
    initialValue: props.goal.completeDate
      ? new Date(props.goal.completeDate)
      : null,
  });

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

  const queryClient = useQueryClient();
  const doEditGoal = useMutation({
    mutationFn: async (newGoal: IGoalUpdateRequest) =>
      await request({
        url: "/api/goal",
        method: "PUT",
        data: newGoal,
      }),
    onMutate: async (variables: IGoalUpdateRequest) => {
      await queryClient.cancelQueries({
        queryKey: ["goals", { includeInterest: props.includeInterest }],
      });

      const previousGoals: IGoalResponse[] =
        queryClient.getQueryData([
          "goals",
          { includeInterest: props.includeInterest },
        ]) ?? [];

      queryClient.setQueryData(
        ["goals", { includeInterest: props.includeInterest }],
        (oldGoals: IGoalResponse[]) =>
          oldGoals?.map((oldGoal: IGoalResponse) =>
            oldGoal.id === variables.id
              ? {
                  ...oldGoal,
                  name: variables.name,
                  completeDate: variables.completeDate,
                  amount: variables.amount,
                  monthlyContribution: variables.monthlyContribution,
                }
              : oldGoal
          )
      );

      return { previousGoals };
    },
    onError: (error: AxiosError, _variables: IGoalUpdateRequest, context) => {
      queryClient.setQueryData(
        ["goals", { includeInterest: props.includeInterest }],
        context?.previousGoals ?? []
      );
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: ["goals", { includeInterest: props.includeInterest }],
      });
    },
  });

  const doCompleteGoal = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/goal/complete",
        method: "POST",
        params: { goalID: id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["goals"],
      });
      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Goal successfully marked as completed",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doDeleteGoal = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/goal",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["goals"],
      });
      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Goal deleted successfully",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const submitChanges = (): void => {
    const newGoal: IGoalUpdateRequest = { ...props.goal };

    if (goalNameField.getValue().length > 0) {
      newGoal.name = goalNameField.getValue();
    } else {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: "Invalid goal name",
      });
    }

    if (goalTargetAmountField.getValue() > 0) {
      newGoal.amount = goalTargetAmountField.getValue();
    } else {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: "Invalid target amount",
      });
    }

    if (goalMonthlyContributionField.getValue() > 0) {
      newGoal.monthlyContribution = goalMonthlyContributionField.getValue();
    } else {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: "Invalid monthly contribution",
      });
    }

    doEditGoal.mutate(newGoal);
  };

  // The DateInput doesn't have an onBlur property, so we need to handle this manually.
  const submitTargetDateChanges = (date: DateValue): void => {
    const parsedDate = dayjs(date);

    if (parsedDate.isValid()) {
      goalTargetDateField.setValue(parsedDate.toDate());
    } else {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: "Invalid target date",
      });
    }

    const newGoal: IGoalUpdateRequest = {
      ...props.goal,
      completeDate: parsedDate.toDate(),
    };

    doEditGoal.mutate(newGoal);
  };

  return (
    <>
      <LoadingOverlay
        visible={
          doEditGoal.isPending ||
          doDeleteGoal.isPending ||
          doCompleteGoal.isPending
        }
      />
      <Group style={{ containerType: "inline-size" }} wrap="nowrap">
        <Stack w="100%" gap="0.25rem">
          <Flex className={classes.header}>
            <Group align="center" gap={10}>
              <TextInput
                {...goalNameField.getInputProps()}
                onBlur={submitChanges}
                onClick={(e) => e.stopPropagation()}
                elevation={1}
              />
              {props.includeInterest && props.goal.interestRate && (
                <Badge variant="light">
                  {props.goal.interestRate.toLocaleString(undefined, {
                    style: "percent",
                    minimumFractionDigits: 2,
                  })}{" "}
                  APR
                </Badge>
              )}
              {/* This is an escape hatch in case the sync does not catch it */}
              {props.goal.percentComplete >= 100 && (
                <Button
                  size="compact-xs"
                  bg="var(--button-color-confirm)"
                  onClick={(e) => {
                    e.stopPropagation();
                    doCompleteGoal.mutate(props.goal.id);
                  }}
                  loading={doCompleteGoal.isPending}
                >
                  Complete Goal
                </Button>
              )}
              <ActionIcon
                variant="outline"
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
              <Flex
                onClick={(e) => {
                  e.stopPropagation();
                }}
              >
                {props.goal.amount !== 0 ? (
                  <NumberInput
                    maw={100}
                    min={0}
                    prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                    thousandSeparator=","
                    {...goalTargetAmountField.getInputProps()}
                    onBlur={submitChanges}
                    elevation={1}
                  />
                ) : (
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
                )}
              </Flex>
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
                {props.goal.isCompleteDateEditable ? (
                  <Flex
                    onClick={(e) => {
                      e.stopPropagation();
                    }}
                  >
                    <DateInput
                      className="h-8"
                      {...goalTargetDateField.getInputProps()}
                      onChange={submitTargetDateChanges}
                    />
                  </Flex>
                ) : (
                  <PrimaryText size="sm">
                    {new Date(props.goal.completeDate).toLocaleDateString(
                      "en-US",
                      {
                        year: "numeric",
                        month: "long",
                      }
                    )}
                  </PrimaryText>
                )}
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
              {props.goal.isMonthlyContributionEditable ? (
                <Flex onClick={(e) => e.stopPropagation()}>
                  <NumberInput
                    size="sm"
                    maw={100}
                    min={0}
                    prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                    thousandSeparator=","
                    {...goalMonthlyContributionField.getInputProps()}
                    onBlur={submitChanges}
                    elevation={1}
                  />
                </Flex>
              ) : userSettingsQuery.isPending ? null : (
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
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="var(--button-color-destructive)"
            onClick={(e) => {
              e.stopPropagation();
              doDeleteGoal.mutate(props.goal.id);
            }}
            h="100%"
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </>
  );
};

export default EditableGoalCardContent;
