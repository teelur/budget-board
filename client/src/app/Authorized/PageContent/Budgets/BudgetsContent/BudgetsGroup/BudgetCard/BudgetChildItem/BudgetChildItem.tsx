import classes from "./BudgetChildItem.module.css";

import { Group, Stack } from "@mantine/core";
import {
  ActionIcon,
  AmountText,
  BodyText,
  Progress,
} from "@teelur/budget-board-ui";
import { PencilIcon, TrashIcon } from "lucide-react";
import { Trans, useTranslation } from "react-i18next";
import CategoryIconPicker from "~/components/CategoryIconPicker/CategoryIconPicker";
import BudgetMetrics from "../BudgetMetrics/BudgetMetrics";
import RolloverButton from "../RolloverButton/RolloverButton";
import { useDisclosure } from "@mantine/hooks";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import { useField } from "@mantine/form";
import { SignDisplay } from "~/helpers/currency";
import BudgetLimitInput from "../BudgetLimitInput/BudgetLimitInput";
import { roundAwayFromZero } from "~/helpers/utils";

interface BudgetChildItemProps {
  id: string;
  categoryValue: string;
  amount: number;
  projectedAmount?: number;
  limit: number;
  rollover: number;
  rolloverStartMonth: string | null;
  rolloverDisabledReason?: string;
  isIncome: boolean;
  icon: string;
  selectedDate: Date;
}

const BudgetChildItem = (props: BudgetChildItemProps): React.ReactNode => {
  const [isEditing, { toggle: toggleEdit }] = useDisclosure(false);

  const { t } = useTranslation();
  const { intlLocale } = useLocale();
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
              ((projectedAmount * (props.isIncome ? 1 : -1)) / availableLimit) *
                100,
            ),
          ),
        );
  const projectedProgressValue = Math.max(
    0,
    projectedPercentComplete - actualProgressValue,
  );

  const getElementForLimit = () => {
    if (isEditing) {
      return (
        <BudgetLimitInput
          field={newLimitField}
          onSave={(newLimit) => handleEdit(newLimit)}
          min={0}
          key="total-edit"
        />
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
    <Group pl="1rem" gap="0.75rem" align="flex-start" wrap="nowrap">
      <Stack gap={0} w="100%">
        <Group
          justify="space-between"
          align="center"
          style={{ containerType: "inline-size" }}
        >
          <Group gap="0.25rem" align="center">
            {isEditing && (
              <CategoryIconPicker
                category={props.categoryValue}
                icon={props.icon}
                size="compact-xs"
              />
            )}
            <BodyText tone="primary" fw={500} className={classes.title}>
              {!isEditing && props.icon.length > 0 ? `${props.icon} ` : ""}
              {props.categoryValue}
            </BodyText>
            {props.id.length > 0 && (
              <ActionIcon
                variant="ghost"
                color="primary"
                size="compact-xs"
                selected={isEditing}
                onClick={(e) => {
                  e.stopPropagation();
                  if (props.id.length > 0) {
                    newLimitField.setValue(props.limit);
                    toggleEdit();
                  }
                }}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            )}
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
                <BodyText tone="secondary" className={classes.text} key="of" />,
                getElementForLimit(),
              ]}
            />
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
      {isEditing && (
        <Group style={{ alignSelf: "stretch" }} gap="0.25rem" wrap="nowrap">
          <RolloverButton
            category={props.categoryValue}
            rolloverStartMonth={props.rolloverStartMonth}
            budgetMonth={props.selectedDate}
            onChange={(newStartMonth) =>
              handleEdit(newLimitField.getValue(), newStartMonth)
            }
            disabledReason={props.rolloverDisabledReason}
          />
          <ActionIcon
            variant="filled"
            color="error"
            size="compact-sm"
            h="100%"
            disabled={deleteBudgetMutation.isPending}
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
  );
};

export default BudgetChildItem;
