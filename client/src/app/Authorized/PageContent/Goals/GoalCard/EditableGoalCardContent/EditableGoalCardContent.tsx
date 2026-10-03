import classes from "./EditableGoalCardContent.module.css";

import { Flex, Group, LoadingOverlay, Stack } from "@mantine/core";
import {
  ActionIcon,
  Badge,
  Button,
  AmountText,
  Progress,
  NumberInput,
  TextInput,
  DateInput,
} from "@teelur/budget-board-ui";
import React from "react";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { getCurrencySymbol, SignDisplay } from "~/helpers/currency";
import { IGoalResponse } from "~/models/goal";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { PencilIcon, TrashIcon } from "lucide-react";
import { useField } from "@mantine/form";
import { DateValue } from "@mantine/dates";
import { getGoalTargetAmount } from "~/helpers/goals";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { StatusColorType } from "~/helpers/budgets";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCompleteGoalMutation } from "~/hooks/mutations/goals/useCompleteGoalMutation";
import { useUpdateGoalMutation } from "~/hooks/mutations/goals/useUpdateGoalMutation";
import { useDeleteGoalMutation } from "~/hooks/mutations/goals/useDeleteGoalMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

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
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
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

  const getElementForMonthlyContribution = () => {
    if (props.goal.isMonthlyContributionEditable) {
      return (
        <Flex onClick={(e) => e.stopPropagation()}>
          <NumberInput
            size="xs"
            maw={100}
            min={0}
            prefix={getCurrencySymbol(preferredCurrency)}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            decimalScale={decimalPlaces}
            {...goalMonthlyContributionField.getInputProps()}
            onBlur={() => {
              if (goalMonthlyContributionField.getValue() > 0) {
                updateGoalMutation.mutate({
                  id: props.goal.id,
                  monthlyContribution: goalMonthlyContributionField.getValue(),
                });
              } else {
                showNotification({
                  type: NotificationType.Error,
                  message: t("invalid_monthly_contribution"),
                });
              }
            }}
          />
        </Flex>
      );
    }

    return (
      <AmountText
        amount={props.goal.monthlyContribution}
        size="md"
        disableStatusColor
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={0}
        key="total-not-edit"
      />
    );
  };

  const getElementForCompleteDate = () => {
    if (props.goal.isCompleteDateEditable) {
      return (
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
                  completeDate: parsedDate.format("YYYY-MM-DD"),
                });
              } else {
                showNotification({
                  type: NotificationType.Error,
                  message: t("invalid_target_date"),
                });
              }
            }}
          />
        </Flex>
      );
    }

    return (
      <PrimaryText size="sm" key="date-not-edit">
        {dayjs(props.goal.completeDate).format("MMMM YYYY")}
      </PrimaryText>
    );
  };

  const getElementForTargetAmount = () => {
    if (props.goal.amount !== 0) {
      return (
        <Flex
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          <NumberInput
            maw={100}
            min={0}
            prefix={getCurrencySymbol(preferredCurrency)}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            decimalScale={decimalPlaces}
            {...goalTargetAmountField.getInputProps()}
            onBlur={() => {
              if (goalTargetAmountField.getValue() > 0) {
                updateGoalMutation.mutate({
                  id: props.goal.id,
                  amount: goalTargetAmountField.getValue(),
                });
              } else {
                showNotification({
                  type: NotificationType.Error,
                  message: t("invalid_target_amount"),
                });
              }
            }}
          />
        </Flex>
      );
    }

    return (
      <AmountText
        amount={getGoalTargetAmount(
          props.goal.amount,
          props.goal.initialAmount,
        )}
        disableStatusColor
        size="lg"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={0}
        signDisplay={SignDisplay.Auto}
        invertSign={false}
        key="total"
      />
    );
  };

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
                    showNotification({
                      type: NotificationType.Error,
                      message: t("invalid_goal_name"),
                    });
                  }
                }}
                onClick={(e) => e.stopPropagation()}
              />
              {props.includeInterest && props.goal.interestRate && (
                <Badge variant="light" color="primary" size="xs">
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
                  variant="filled"
                  color="success"
                  size="sm"
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
                color="primary"
                size="compact-sm"
                onClick={(e) => {
                  e.stopPropagation();
                  props.toggleIsSelected();
                }}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            </Group>
            <Flex justify="flex-end" align="center" gap="0.25rem">
              <Trans
                i18nKey="x_of_y"
                components={[
                  <AmountText
                    amount={
                      sumAccountsTotalBalance(props.goal.accounts) -
                      props.goal.initialAmount
                    }
                    size="lg"
                    disableStatusColor
                    isSensitive={isPrivacyModeEnabled}
                    locale={intlLocale}
                    currency={preferredCurrency}
                    decimalPlaces={0}
                    key="amount"
                  />,
                  <DimmedText size="sm" key="of" />,
                  getElementForTargetAmount(),
                ]}
              />
            </Flex>
          </Flex>
          <Progress
            value={props.goal.percentComplete}
            size="md"
            label
            ariaLabel={props.goal.name}
          />
          <Flex className={classes.footer}>
            <Group align="center" gap="sm">
              <Flex align="center" gap="0.25rem">
                <DimmedText size="sm">{t("projected_colon")}</DimmedText>
                {getElementForCompleteDate()}
              </Flex>
            </Group>
            <Flex justify="flex-end" align="center" gap="0.25rem">
              <Trans
                i18nKey="x_of_y_this_month"
                components={[
                  <AmountText
                    amount={props.goal.monthlyContributionProgress}
                    size="md"
                    total={props.goal.monthlyContribution}
                    type={StatusColorType.Target}
                    isSensitive={isPrivacyModeEnabled}
                    locale={intlLocale}
                    currency={preferredCurrency}
                    decimalPlaces={decimalPlaces}
                    key="amount"
                  />,
                  <DimmedText size="sm" key="of" />,
                  getElementForMonthlyContribution(),
                  <DimmedText size="sm" key="this_month" />,
                ]}
              />
            </Flex>
          </Flex>
        </Stack>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon
            variant="filled"
            color="error"
            size="compact-sm"
            h="100%"
            onClick={(e) => {
              e.stopPropagation();
              deleteGoalMutation.mutate(props.goal.id);
            }}
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </>
  );
};

export default EditableGoalCardContent;
