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
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import {
  convertNumberToCurrency,
  getCurrencySymbol,
  SignDisplay,
} from "~/helpers/currency";
import { IGoalResponse, IGoalUpdateRequest } from "~/models/goal";
import { IUserSettings } from "~/models/userSettings";
import { notifications } from "@mantine/notifications";
import { userSettingsQueryKey } from "~/helpers/requests";
import { PencilIcon, TrashIcon } from "lucide-react";
import { useField } from "@mantine/form";
import { DateValue } from "@mantine/dates";
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
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCompleteGoalMutation } from "~/hooks/mutations/goals/useCompleteGoalMutation";
import { useUpdateGoalMutation } from "~/hooks/mutations/goals/useUpdateGoalMutation";
import { useDeleteGoalMutation } from "~/hooks/mutations/goals/useDeleteGoalMutation";

interface GoalCardContentProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  toggleIsSelected: () => void;
}

const EditableGoalCardContent = (
  props: GoalCardContentProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    intlLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { request } = useAuth();
  const updateGoalMutation = useUpdateGoalMutation();
  const deleteGoalMutation = useDeleteGoalMutation();
  const completeGoalMutation = useCompleteGoalMutation();

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
    initialValue: dayjs(props.goal.completeDate).isValid()
      ? props.goal.completeDate
      : null,
  });

  const userSettingsQuery = useQuery({
    queryKey: [userSettingsQueryKey],
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
    <>
      <LoadingOverlay
        visible={
          updateGoalMutation.isPending ||
          deleteGoalMutation.isPending ||
          completeGoalMutation.isPending
        }
      />
      <Group style={{ containerType: "inline-size" }} wrap="nowrap">
        <Stack w="100%" gap="0.25rem">
          <Flex className={classes.header}>
            <Group align="center" gap={10}>
              <TextInput
                {...goalNameField.getInputProps()}
                onBlur={(event) => {
                  if (event.currentTarget.value.length > 0) {
                    goalNameField.setValue(event.currentTarget.value);
                    updateGoalMutation.mutate({
                      id: props.goal.id,
                      name: event.currentTarget.value,
                    });
                  } else {
                    notifications.show({
                      color: "var(--button-color-destructive)",
                      message: t("invalid_goal_name"),
                    });
                  }
                }}
                onClick={(e) => e.stopPropagation()}
                elevation={1}
              />
              {props.includeInterest && props.goal.interestRate && (
                <Badge variant="light">
                  {t("interest_rate_apr", {
                    rate: new Intl.NumberFormat(intlLocale, {
                      style: "percent",
                      maximumFractionDigits: 2,
                    }).format(props.goal.interestRate),
                  })}
                </Badge>
              )}
              {/* This is an escape hatch in case the sync does not catch it */}
              {props.goal.percentComplete >= 100 && (
                <Button
                  size="compact-xs"
                  bg="var(--button-color-confirm)"
                  onClick={(e) => {
                    e.stopPropagation();
                    completeGoalMutation.mutate(props.goal.id);
                  }}
                  loading={completeGoalMutation.isPending}
                >
                  {t("mark_as_complete")}
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
              {props.goal.amount !== 0 ? (
                <>
                  <Trans
                    i18nKey="budget_amount_fraction_editable_total_styled"
                    values={{
                      amount: convertNumberToCurrency(
                        sumAccountsTotalBalance(props.goal.accounts) -
                          props.goal.initialAmount,
                        false,
                        userSettingsQuery.data?.currency ?? "USD",
                        SignDisplay.Auto,
                        intlLocale,
                      ),
                    }}
                    components={[
                      <PrimaryText size="lg" key="amount" />,
                      <DimmedText size="sm" key="of" />,
                    ]}
                  />
                  <Flex
                    onClick={(e) => {
                      e.stopPropagation();
                    }}
                  >
                    <NumberInput
                      maw={100}
                      min={0}
                      prefix={getCurrencySymbol(
                        userSettingsQuery.data?.currency,
                      )}
                      thousandSeparator={thousandsSeparator}
                      decimalSeparator={decimalSeparator}
                      {...goalTargetAmountField.getInputProps()}
                      onBlur={(event) => {
                        if (event.currentTarget.valueAsNumber > 0) {
                          goalTargetAmountField.setValue(
                            event.currentTarget.valueAsNumber,
                          );
                          updateGoalMutation.mutate({
                            id: props.goal.id,
                            amount: event.currentTarget.valueAsNumber,
                          });
                        } else {
                          notifications.show({
                            color: "var(--button-color-destructive)",
                            message: t("invalid_target_amount"),
                          });
                        }
                      }}
                      elevation={1}
                    />
                  </Flex>
                </>
              ) : (
                <Trans
                  i18nKey="budget_amount_fraction_styled"
                  values={{
                    amount: convertNumberToCurrency(
                      sumAccountsTotalBalance(props.goal.accounts) -
                        props.goal.initialAmount,
                      false,
                      userSettingsQuery.data?.currency ?? "USD",
                      SignDisplay.Auto,
                      intlLocale,
                    ),
                    total: convertNumberToCurrency(
                      getGoalTargetAmount(
                        props.goal.amount,
                        props.goal.initialAmount,
                      ),
                      false,
                      userSettingsQuery.data?.currency ?? "USD",
                      SignDisplay.Auto,
                      intlLocale,
                    ),
                  }}
                  components={[
                    <PrimaryText size="lg" key="amount" />,
                    <DimmedText size="md" key="of" />,
                    <PrimaryText size="lg" key="total" />,
                  ]}
                />
              )}
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
                {props.goal.isCompleteDateEditable ? (
                  <>
                    <Trans
                      i18nKey="budget_projected_editable_styled"
                      values={{
                        amount: convertNumberToCurrency(
                          sumAccountsTotalBalance(props.goal.accounts) -
                            props.goal.initialAmount,
                          false,
                          userSettingsQuery.data?.currency ?? "USD",
                          SignDisplay.Auto,
                          intlLocale,
                        ),
                      }}
                      components={[<DimmedText size="sm" key="label" />]}
                    />
                    <Flex
                      onClick={(e) => {
                        e.stopPropagation();
                      }}
                    >
                      <DateInput
                        className="h-8"
                        {...goalTargetDateField.getInputProps()}
                        locale={dayjsLocale}
                        valueFormat={longDateFormat}
                        onChange={(date) => {
                          const parsedDate = dayjs(date);

                          if (parsedDate.isValid()) {
                            goalTargetDateField.setValue(parsedDate.toDate());
                            updateGoalMutation.mutate({
                              id: props.goal.id,
                              completeDate: parsedDate.toDate(),
                            });
                          } else {
                            notifications.show({
                              color: "var(--button-color-destructive)",
                              message: t("invalid_target_date"),
                            });
                          }
                        }}
                      />
                    </Flex>
                  </>
                ) : (
                  <Trans
                    i18nKey="budget_projected_styled"
                    values={{
                      amount: dayjs(props.goal.completeDate).format(
                        "MMMM YYYY",
                      ),
                    }}
                    components={[
                      <DimmedText size="sm" key="label" />,
                      <PrimaryText size="sm" key="date-not-edit" />,
                    ]}
                  />
                )}
              </Flex>
            </Group>
            <Flex justify="flex-end" align="center" gap="0.25rem">
              {props.goal.isMonthlyContributionEditable ? (
                <>
                  <Trans
                    i18nKey="budget_monthly_amount_fraction_editable_styled"
                    values={{
                      amount: convertNumberToCurrency(
                        sumAccountsTotalBalance(props.goal.accounts) -
                          props.goal.initialAmount,
                        false,
                        userSettingsQuery.data?.currency ?? "USD",
                        SignDisplay.Auto,
                        intlLocale,
                      ),
                    }}
                    components={[
                      <StatusText
                        amount={props.goal.monthlyContributionProgress}
                        total={props.goal.monthlyContribution}
                        type={StatusColorType.Target}
                        size="md"
                        key="amount"
                      />,
                      <DimmedText size="sm" key="of" />,
                    ]}
                  />
                  <Flex onClick={(e) => e.stopPropagation()}>
                    <NumberInput
                      size="sm"
                      maw={100}
                      min={0}
                      prefix={getCurrencySymbol(
                        userSettingsQuery.data?.currency,
                      )}
                      thousandSeparator={thousandsSeparator}
                      decimalSeparator={decimalSeparator}
                      {...goalMonthlyContributionField.getInputProps()}
                      onBlur={(event) => {
                        if (event.currentTarget.valueAsNumber > 0) {
                          goalMonthlyContributionField.setValue(
                            event.currentTarget.valueAsNumber,
                          );
                          updateGoalMutation.mutate({
                            id: props.goal.id,
                            monthlyContribution:
                              event.currentTarget.valueAsNumber,
                          });
                        } else {
                          notifications.show({
                            color: "var(--button-color-destructive)",
                            message: t("invalid_monthly_contribution"),
                          });
                        }
                      }}
                      elevation={1}
                    />
                  </Flex>
                  <DimmedText size="sm">
                    {t("editable_amount_fraction_this_month")}
                  </DimmedText>
                </>
              ) : (
                <Trans
                  i18nKey="budget_monthly_amount_fraction_styled"
                  values={{
                    amount: convertNumberToCurrency(
                      props.goal.monthlyContributionProgress,
                      false,
                      userSettingsQuery.data?.currency ?? "USD",
                      SignDisplay.Auto,
                      intlLocale,
                    ),
                    total: convertNumberToCurrency(
                      props.goal.monthlyContribution,
                      false,
                      userSettingsQuery.data?.currency ?? "USD",
                      SignDisplay.Auto,
                      intlLocale,
                    ),
                  }}
                  components={[
                    <StatusText
                      amount={props.goal.monthlyContributionProgress}
                      total={props.goal.monthlyContribution}
                      type={StatusColorType.Target}
                      size="md"
                      key="amount"
                    />,
                    <DimmedText size="sm" key="of" />,
                    <PrimaryText size="md" key="total-not-edit" />,
                  ]}
                />
              )}
            </Flex>
          </Flex>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="var(--button-color-destructive)"
            onClick={(e) => {
              e.stopPropagation();
              deleteGoalMutation.mutate(props.goal.id);
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
