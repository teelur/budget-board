import {
  ActionIcon,
  Badge,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  NumberInput,
  Progress,
  Stack,
  Text,
  TextInput,
} from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import { IGoalResponse, IGoalUpdateRequest } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";
import GoalDetails from "../GoalDetails/GoalDetails";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { PencilIcon, TrashIcon } from "lucide-react";
import { useField } from "@mantine/form";
import { DatePickerInput, DateValue } from "@mantine/dates";
import dayjs from "dayjs";

interface GoalCardContentProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  toggleIsSelected: () => void;
}

const EditableGoalCardContent = (
  props: GoalCardContentProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
      notifications.show({ color: "red", message: translateAxiosError(error) });
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
        color: "green",
        message: "Goal successfully marked as completed",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "red",
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
        color: "green",
        message: "Goal deleted successfully",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "red",
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
        color: "red",
        message: "Invalid goal name",
      });
    }

    if (goalTargetAmountField.getValue() > 0) {
      newGoal.amount = goalTargetAmountField.getValue();
    } else {
      notifications.show({
        color: "red",
        message: "Invalid target amount",
      });
    }

    if (goalMonthlyContributionField.getValue() > 0) {
      newGoal.monthlyContribution = goalMonthlyContributionField.getValue();
    } else {
      notifications.show({
        color: "red",
        message: "Invalid monthly contribution",
      });
    }

    doEditGoal.mutate(newGoal);
  };

  // The DateInput doesn't have an onBlur property, so we need to handle this manually.
  const submitTargetDateChanges = (date: DateValue): void => {
    if (date) {
      goalTargetDateField.setValue(date);

      const parsedDate = dayjs(date);

      const newGoal: IGoalUpdateRequest = {
        ...props.goal,
        completeDate: parsedDate.toDate(),
      };

      doEditGoal.mutate(newGoal);
    } else {
      notifications.show({
        color: "red",
        message: "Invalid target date",
      });
    }
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
      <Group wrap="nowrap">
        <Stack w="100%" gap="0.1rem">
          <Flex direction="row" justify="space-between">
            <Group align="center" gap={10}>
              <TextInput
                {...goalNameField.getInputProps()}
                onBlur={submitChanges}
                onClick={(e) => e.stopPropagation()}
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
                  bg="green"
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
              <Flex
                onClick={(e) => {
                  e.stopPropagation();
                }}
              >
                <NumberInput
                  maw={100}
                  min={0}
                  prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                  thousandSeparator=","
                  {...goalTargetAmountField.getInputProps()}
                  onBlur={submitChanges}
                />
              </Flex>
            </Flex>
          </Flex>
          <Progress.Root size={18} radius="xl">
            <Progress.Section value={props.goal.percentComplete}>
              <Progress.Label>
                {props.goal.percentComplete.toFixed(0)}%
              </Progress.Label>
            </Progress.Section>
          </Progress.Root>
          <Flex direction="row" justify="space-between">
            <Group align="center" gap="sm">
              <Flex align="center" gap="0.25rem">
                <Text size="sm" fw={600} c="dimmed">
                  {"Projected: "}
                </Text>
                {props.goal.isCompleteDateEditable ? (
                  <Flex
                    onClick={(e) => {
                      e.stopPropagation();
                    }}
                  >
                    <DatePickerInput
                      className="h-8"
                      {...goalTargetDateField.getInputProps()}
                      onChange={submitTargetDateChanges}
                    />
                  </Flex>
                ) : (
                  <Text size="sm" fw={600} c="dimmed">
                    {new Date(props.goal.completeDate).toLocaleDateString(
                      "en-US",
                      {
                        year: "numeric",
                        month: "long",
                      }
                    )}
                  </Text>
                )}
              </Flex>
              <GoalDetails goal={props.goal} />
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
              {props.goal.isMonthlyContributionEditable ? (
                <Flex onClick={(e) => e.stopPropagation()}>
                  <NumberInput
                    maw={100}
                    min={0}
                    prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                    thousandSeparator=","
                    {...goalMonthlyContributionField.getInputProps()}
                    onBlur={submitChanges}
                  />
                </Flex>
              ) : userSettingsQuery.isPending ? null : (
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
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="red"
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
