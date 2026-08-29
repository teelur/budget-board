import classes from "./BudgetChildCard.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import { getCurrencySymbol, SignDisplay } from "~/helpers/currency";
import {
  ActionIcon,
  Box,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import React from "react";
import { useField } from "@mantine/form";
import { PencilIcon, TrashIcon } from "lucide-react";
import { roundAwayFromZero } from "~/helpers/utils";
import { useDisclosure } from "@mantine/hooks";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import BudgetMetrics from "../BudgetMetrics/BudgetMetrics";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface BudgetChildCardProps {
  id: string;
  categoryValue: string;
  amount: number;
  projectedAmount?: number;
  limit: number;
  isIncome: boolean;
  selectedDate: Date;
  openDetails: (category: string, month: Date) => void;
}

const BudgetChildCard = (props: BudgetChildCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const formatAmount = useSensitiveAmountFormatter();
  const formatSensitiveAmount = (amount: number): string =>
    formatAmount(amount, false, SignDisplay.Auto);
  const updateBudgetMutation = useUpdateBudgetMutation();
  const deleteBudgetMutation = useDeleteBudgetMutation();
  const projectedAmount = props.projectedAmount ?? props.amount;

  const newLimitField = useField<number | string>({
    initialValue: props.limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (props.id.length === 0) {
      return;
    }
    updateBudgetMutation.mutate({
      id: props.id,
      limit: Number(newLimit),
    });
  };

  const percentComplete = roundAwayFromZero(
    ((props.amount * (props.isIncome ? 1 : -1)) / props.limit) * 100,
  );
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
              <PrimaryText className={classes.title} elevation={1}>
                {props.categoryValue}
              </PrimaryText>
              <ActionIcon
                variant={isSelected ? "outline" : "transparent"}
                size="sm"
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
              {isSelected ? (
                <>
                  <Trans
                    i18nKey="budget_amount_fraction_editable_total_styled"
                    values={{
                      amount: formatSensitiveAmount(
                        props.amount * (props.isIncome ? 1 : -1),
                      ),
                      total: formatSensitiveAmount(props.limit),
                    }}
                    components={[
                      <PrimaryText
                        className={classes.text}
                        key="amount"
                        elevation={1}
                      />,
                      <DimmedText size="sm" key="of" elevation={1} />,
                    ]}
                  />
                  <Flex onClick={(e) => e.stopPropagation()}>
                    <NumberInput
                      {...newLimitField.getInputProps()}
                      onBlur={() => handleEdit(newLimitField.getValue())}
                      thousandSeparator={thousandsSeparator}
                      decimalSeparator={decimalSeparator}
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
                </>
              ) : (
                <Trans
                  i18nKey="budget_amount_fraction_styled"
                  values={{
                    amount: formatSensitiveAmount(
                      props.amount * (props.isIncome ? 1 : -1),
                    ),
                    total: formatSensitiveAmount(props.limit),
                  }}
                  components={[
                    <PrimaryText
                      className={classes.text}
                      key="amount"
                      elevation={1}
                    />,
                    <DimmedText size="sm" key="of" elevation={1} />,
                    <PrimaryText
                      className={classes.text}
                      key="total"
                      elevation={1}
                    />,
                  ]}
                />
              )}
            </Group>
          </Group>
          <Group
            gap="0.5rem"
            align="center"
            style={{ containerType: "inline-size" }}
          >
            <Flex style={{ flex: "1 1 auto", minWidth: 0 }}>
              <Progress
                size={10}
                percentComplete={percentComplete}
                amount={props.amount}
                limit={props.limit}
                projectedAmount={projectedAmount}
                type={
                  props.isIncome ? ProgressType.Income : ProgressType.Expense
                }
                warningThreshold={budgetWarningThreshold}
                elevation={1}
                showPercentLabel={false}
              />
            </Flex>
            <PrimaryText
              size="sm"
              elevation={1}
              style={{ flexShrink: 0, lineHeight: 1 }}
            >
              {percentComplete.toFixed(0)}%
            </PrimaryText>
          </Group>
          <BudgetMetrics
            amount={props.amount}
            projectedAmount={projectedAmount}
            limit={props.limit}
            isIncome={props.isIncome}
            budgetWarningThreshold={budgetWarningThreshold}
            formatAmount={formatSensitiveAmount}
          />
        </Stack>
        {isSelected && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              color="var(--button-color-destructive)"
              onClick={(e) => {
                e.stopPropagation();
                deleteBudgetMutation.mutate(props.id);
              }}
              h="100%"
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
