import classes from "./BudgetParentItem.module.css";

import { Flex, Group, Stack } from "@mantine/core";
import {
  ActionIcon,
  AmountText,
  BodyText,
  Button,
  HeadingText,
  Popover,
  Progress,
} from "@teelur/budget-board-ui";
import { ChevronDown, PencilIcon, TrashIcon } from "lucide-react";
import { Trans, useTranslation } from "react-i18next";
import CategoryIconPicker from "~/components/CategoryIconPicker/CategoryIconPicker";
import BudgetMetrics from "../BudgetMetrics/BudgetMetrics";
import RolloverButton from "../RolloverButton/RolloverButton";
import { type ICategoryNode } from "~/models/category";
import { getCategoryIcon } from "~/helpers/category";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useDisclosure } from "@mantine/hooks";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { SignDisplay } from "~/helpers/currency";
import { roundAwayFromZero } from "~/helpers/utils";
import BudgetLimitInput from "../BudgetLimitInput/BudgetLimitInput";
import { useField } from "@mantine/form";
import { IBudget } from "~/models/budget";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import React from "react";

const getProjectedProgressValue = (
  amount: number,
  projectedAmount: number,
  availableLimit: number,
  isIncome: boolean,
) => {
  const percentComplete = roundAwayFromZero(
    ((amount * (isIncome ? 1 : -1)) / availableLimit) * 100,
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
              ((projectedAmount * (isIncome ? 1 : -1)) / availableLimit) * 100,
            ),
          ),
        );

  return Math.max(0, projectedPercentComplete - actualProgressValue);
};

interface BudgetParentItemProps {
  categoryNode: ICategoryNode;
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  categoryToLimitsMap: Map<string, number>;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToRolloverMap: Map<string, number>;
  selectedDate: Date | null;
  isCollapsed: boolean;
  isIncome: boolean;
  toggleCollapsed: () => void;
  hasChildren: boolean;
}

const BudgetParentItem = (props: BudgetParentItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { intlLocale, dayjs } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const { allTransactionCategories } = useTransactionCategories();

  const updateBudgetMutation = useUpdateBudgetMutation();
  const deleteBudgetMutation = useDeleteBudgetMutation();

  const [isEditing, { toggle: toggleEditing, close: closeEditing }] =
    useDisclosure(false);

  const categoryKey = props.categoryNode.value.toLowerCase();
  const categoryIcon = getCategoryIcon(
    props.categoryNode.value,
    allTransactionCategories,
  );

  // This is technically just a single budget, but there may be multiple instances
  // if multiple months are selected.
  const budgets = props.categoryToBudgetsMap.get(categoryKey) ?? [];
  const budget = props.selectedDate && budgets.length === 1 ? budgets[0] : null;

  const amount = props.categoryToTransactionsTotalMap.get(categoryKey) ?? 0;
  const projectedAmount =
    amount + (props.categoryToRecurringForecastTotalMap.get(categoryKey) ?? 0);

  const rollover = props.categoryToRolloverMap.get(categoryKey) ?? 0;
  const rolloverStartMonth = budgets[0]?.rolloverStartMonth ?? null;
  const parentRollsOver = budgets.some(
    (budget) => budget.rolloverStartMonth !== null,
  );
  const hasRolloverChildren = props.categoryNode.subCategories.some(
    (subCategory) =>
      (
        props.categoryToBudgetsMap.get(subCategory.value.toLowerCase()) ?? []
      ).some((budget) => budget.rolloverStartMonth !== null),
  );

  const limit = props.categoryToLimitsMap.get(categoryKey) ?? 0;
  const availableLimit = limit + rollover;
  const childLimitsTotal = props.categoryNode.subCategories.reduce(
    (acc, subCategory) => {
      const limit =
        props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ?? 0;
      return acc + limit;
    },
    0,
  );
  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const projectedProgressValue = React.useMemo(
    () =>
      getProjectedProgressValue(
        amount,
        projectedAmount,
        availableLimit,
        props.isIncome,
      ),
    [amount, projectedAmount, availableLimit, props.isIncome],
  );

  const getElementForLimit = () => {
    if (isEditing) {
      return (
        <BudgetLimitInput
          field={newLimitField}
          onSave={(newLimit) => handleEdit(newLimit)}
          min={childLimitsTotal}
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
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={0}
        signDisplay={SignDisplay.Auto}
        className={classes.text}
        key="total"
      />
    );
  };

  const handleEdit = (
    newLimit?: number | string,
    newRolloverStartMonth?: string | null,
  ) => {
    if (newLimit === "") {
      return;
    }
    if (!budget) {
      return;
    }
    updateBudgetMutation.mutate({
      id: budget.id,
      limit: Number(newLimit),
      rolloverStartMonth:
        newRolloverStartMonth === undefined
          ? rolloverStartMonth
          : newRolloverStartMonth,
    });
  };

  return (
    <Group gap="0.75rem" align="flex-start" wrap="nowrap">
      <Stack gap={0} w="100%">
        <Group
          justify="space-between"
          align="center"
          style={{ containerType: "inline-size" }}
          gap={0}
        >
          <Group gap="0.25rem" align="center">
            {props.hasChildren && (
              <ActionIcon
                variant="ghost"
                color="primary"
                size="compact-xs"
                aria-label={t("toggle_budget_category", {
                  category: props.categoryNode.value,
                })}
                aria-expanded={!props.isCollapsed}
                onClick={(e) => {
                  e.stopPropagation();
                  props.toggleCollapsed();
                }}
              >
                <ChevronDown
                  className={
                    props.isCollapsed ? classes.collapseIcon : undefined
                  }
                  size={18}
                />
              </ActionIcon>
            )}
            {isEditing && (
              <CategoryIconPicker
                category={props.categoryNode.value}
                icon={categoryIcon}
              />
            )}
            <HeadingText tone="primary" className={classes.title}>
              {!isEditing && categoryIcon.length > 0 ? `${categoryIcon} ` : ""}
              {props.categoryNode.value}
            </HeadingText>
            {budget && (
              <ActionIcon
                variant="ghost"
                color="primary"
                size="compact-xs"
                selected={isEditing}
                onClick={(e) => {
                  e.stopPropagation();
                  if (budget) {
                    newLimitField.setValue(limit);
                    toggleEditing();
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
                  amount={amount}
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
                <BodyText tone="secondary" size="sm" key="of" />,
                getElementForLimit(),
              ]}
            />
          </Group>
        </Group>
        <Progress
          amount={amount}
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
          size="sm"
          type={props.isIncome ? "income" : "expense"}
          warningThreshold={budgetWarningThreshold}
          ariaLabel={props.categoryNode.value}
        />
        <BudgetMetrics
          amount={amount}
          projectedAmount={projectedAmount}
          limit={availableLimit}
          rollover={
            parentRollsOver || hasRolloverChildren ? rollover : undefined
          }
          isIncome={props.isIncome}
        />
      </Stack>
      {isEditing && (
        <Flex
          style={{ alignSelf: "stretch" }}
          gap="0.25rem"
          onClick={(e) => e.stopPropagation()}
        >
          <RolloverButton
            category={props.categoryNode.value}
            rolloverStartMonth={rolloverStartMonth}
            budgetMonth={props.selectedDate ?? dayjs().toDate()}
            onChange={(newStartMonth) =>
              handleEdit(newLimitField.getValue(), newStartMonth)
            }
            disabledReason={
              hasRolloverChildren
                ? t("rollover_on_for_subcategories")
                : undefined
            }
          />
          <Popover>
            <Popover.Target>
              <ActionIcon
                variant="filled"
                color="error"
                size="compact-sm"
                h="100%"
              >
                <TrashIcon size="1rem" />
              </ActionIcon>
            </Popover.Target>
            <Popover.Dropdown maw={200}>
              <Stack gap={5}>
                <BodyText tone="primary" size="sm">
                  {t("confirm_delete_budget_message")}
                </BodyText>
                <BodyText tone="secondary" size="xs">
                  {t("all_children_will_also_be_deleted")}
                </BodyText>
                <Button
                  variant="filled"
                  color="error"
                  size="compact-xs"
                  onClick={() => {
                    deleteBudgetMutation.mutate(budget!.id);
                    closeEditing();
                  }}
                >
                  {t("delete")}
                </Button>
              </Stack>
            </Popover.Dropdown>
          </Popover>
        </Flex>
      )}
    </Group>
  );
};

export default BudgetParentItem;
