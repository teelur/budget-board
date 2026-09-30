import classes from "./BudgetChildCard.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import { getCurrencySymbol, SignDisplay } from "~/helpers/currency";
import { Box, Flex, Group, LoadingOverlay, Stack } from "@mantine/core";
import { ActionIcon, AmountText, Progress } from "@teelur/budget-board-ui";
import React from "react";
import { useField } from "@mantine/form";
import { PencilIcon, TrashIcon } from "lucide-react";
import { roundAwayFromZero } from "~/helpers/utils";
import { useDisclosure } from "@mantine/hooks";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import BudgetMetrics from "../BudgetMetrics/BudgetMetrics";
import RolloverControl from "../RolloverControl/RolloverControl";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import CategoryIconPicker from "~/components/CategoryIconPicker/CategoryIconPicker";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface BudgetChildCardProps {
  id: string;
  categoryValue: string;
  amount: number;
  projectedAmount?: number;
  limit: number;
  rollover: number;
  rolloverStartMonth: string | null;
  isIncome: boolean;
  icon: string;
  selectedDate: Date;
  openDetails: (category: string, month: Date) => void;
}

const BudgetChildCard = (props: BudgetChildCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator, intlLocale } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const updateBudgetMutation = useUpdateBudgetMutation();
  const deleteBudgetMutation = useDeleteBudgetMutation();
  const projectedAmount = props.projectedAmount ?? props.amount;
  const availableLimit = props.limit + props.rollover;

  const newLimitField = useField<number | string>({
    initialValue: props.limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const handleEdit = (
    newLimit?: number | string,
    newRolloverStartMonth?: string | null,
  ) => {
    if (newLimit === "") {
      return;
    }
    if (props.id.length === 0) {
      return;
    }
    updateBudgetMutation.mutate({
      id: props.id,
      limit: Number(newLimit),
      rolloverStartMonth:
        newRolloverStartMonth === undefined
          ? props.rolloverStartMonth
          : newRolloverStartMonth,
    });
  };

  const percentComplete = roundAwayFromZero(
    ((props.amount * (props.isIncome ? 1 : -1)) / availableLimit) * 100,
  );

  const actualProgressValue =
    availableLimit <= 0 ? 0 : Math.min(100, Math.max(0, percentComplete));
  const projectedPercentComplete =
    availableLimit <= 0
      ? actualProgressValue
      : Math.min(
          100,
          Math.max(
            0,
            roundAwayFromZero(
              ((projectedAmount * (props.isIncome ? 1 : -1)) /
                availableLimit) *
                100,
            ),
          ),
        );
  const projectedProgressValue = Math.max(
    0,
    projectedPercentComplete - actualProgressValue,
  );

  const getElementForLimit = () => {
    if (isSelected) {
      return (
        <Flex onClick={(e) => e.stopPropagation()}>
          <NumberInput
            {...newLimitField.getInputProps()}
            onBlur={() => handleEdit(newLimitField.getValue())}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            decimalScale={0}
            min={0}
            max={999999}
            step={1}
            prefix={getCurrencySymbol(preferredCurrency)}
            placeholder={t("enter_limit")}
            size="xs"
            styles={{
              root: {
                maxWidth: "100px",
              },
              input: {
                padding: "0 10px",
                fontSize: "16px",
              },
            }}
            key="total-edit"
            elevation={1}
          />
        </Flex>
      );
    }
    return (
      <AmountText
        amount={availableLimit}
        disableStatusColor
        size="md"
        isSensitive={isPrivacyModeEnabled}
        warningThreshold={budgetWarningThreshold}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={0}
        signDisplay={SignDisplay.Auto}
        className={classes.text}
        key="total"
      />
    );
  };

  return (
    <Box
      mx="0.25rem"
      my="0.125rem"
      p="0.25rem 0.5rem"
      pl="1.5rem"
      data-hover-effect={!isSelected ? "true" : undefined}
      className={`${classes.row} ${hoverClasses.hoverable} ${hoverClasses.outline}`}
      onClick={() => {
        if (props.id.length > 0) {
          props.openDetails(props.categoryValue, props.selectedDate);
        }
      }}
    >
      <LoadingOverlay
        visible={
          updateBudgetMutation.isPending || deleteBudgetMutation.isPending
        }
      />
      <Group gap="0.75rem" align="flex-start" wrap="nowrap">
        <Stack gap={0} w="100%">
          <Group
            justify="space-between"
            align="center"
            style={{ containerType: "inline-size" }}
          >
            <Group gap="0.25rem" align="center">
              {isSelected && (
                <CategoryIconPicker
                  category={props.categoryValue}
                  icon={props.icon}
                  size="compact-xs"
                />
              )}
              <PrimaryText className={classes.title} elevation={1}>
                {!isSelected && props.icon.length > 0 ? `${props.icon} ` : ""}
                {props.categoryValue}
              </PrimaryText>
              <ActionIcon
                variant="ghost"
                color="primary"
                size="compact-xs"
                selected={isSelected}
                onClick={(e) => {
                  e.stopPropagation();
                  if (props.id.length > 0) {
                    newLimitField.setValue(props.limit);
                    toggle();
                  }
                }}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            </Group>
            <Group gap="0.25rem" justify="flex-end" align="center">
              <Trans
                i18nKey="x_of_y"
                components={[
                  <AmountText
                    amount={props.amount}
                    disableStatusColor
                    size="md"
                    isSensitive={isPrivacyModeEnabled}
                    locale={intlLocale}
                    currency={preferredCurrency}
                    decimalPlaces={0}
                    signDisplay={SignDisplay.Auto}
                    invertSign={!props.isIncome}
                    className={classes.text}
                    key="amount"
                  />,
                  <DimmedText size="sm" key="of" elevation={1} />,
                  getElementForLimit(),
                ]}
              />
              {isSelected && (
                <Flex onClick={(e) => e.stopPropagation()}>
                  <RolloverControl
                    rolloverStartMonth={props.rolloverStartMonth}
                    budgetMonth={props.selectedDate}
                    onChange={(newStartMonth) =>
                      handleEdit(newLimitField.getValue(), newStartMonth)
                    }
                  />
                </Flex>
              )}
            </Group>
          </Group>
          <Progress
            amount={props.amount}
            limit={availableLimit}
            label
            sections={
              projectedProgressValue > 0
                ? [
                    {
                      ariaLabel: t("recurring_transactions"),
                      color: "muted",
                      striped: true,
                      value: projectedProgressValue,
                    },
                  ]
                : []
            }
            size="xs"
            type={props.isIncome ? "income" : "expense"}
            warningThreshold={budgetWarningThreshold}
            ariaLabel={props.categoryValue}
          />
          <BudgetMetrics
            amount={props.amount}
            projectedAmount={projectedAmount}
            limit={availableLimit}
            rollover={
              props.rolloverStartMonth !== null ? props.rollover : undefined
            }
            isIncome={props.isIncome}
          />
        </Stack>
        {isSelected && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              variant="filled"
              color="error"
              size="compact-sm"
              h="100%"
              onClick={(e) => {
                e.stopPropagation();
                deleteBudgetMutation.mutate(props.id);
              }}
            >
              <TrashIcon size="1rem" />
            </ActionIcon>
          </Group>
        )}
      </Group>
    </Box>
  );
};

export default BudgetChildCard;
