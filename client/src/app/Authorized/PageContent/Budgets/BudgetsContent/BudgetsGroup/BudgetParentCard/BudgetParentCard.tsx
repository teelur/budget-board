import classes from "./BudgetParentCard.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import { SignDisplay } from "~/helpers/currency";
import {
  Box,
  Flex,
  Group,
  LoadingOverlay,
  Popover as MantinePopover,
  Stack,
} from "@mantine/core";
import {
  ActionIcon,
  AmountText,
  Button,
  Progress,
} from "@teelur/budget-board-ui";
import { IBudget } from "~/models/budget";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import { useField } from "@mantine/form";
import { ChevronDown, PencilIcon, TrashIcon } from "lucide-react";
import { areStringsEqual, roundAwayFromZero } from "~/helpers/utils";
import { CategoryTypes, ICategoryNode } from "~/models/category";
import BudgetChildCard from "./BudgetChildCard/BudgetChildCard";
import UnbudgetChildCard from "./UnbudgetChildCard/UnbudgetChildCard";
import Card from "~/components/core/Card/Card";
import Divider from "~/components/core/Divider/Divider";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Popover from "~/components/core/Popover/Popover";
import BudgetMetrics from "./BudgetMetrics/BudgetMetrics";
import RolloverButton from "./RolloverButton/RolloverButton";
import BudgetLimitInput from "./BudgetLimitInput/BudgetLimitInput";
import { useTranslation, Trans } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import CategoryIconPicker from "~/components/CategoryIconPicker/CategoryIconPicker";
import { getCategoryIcon } from "~/helpers/category";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToRolloverMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
  isCollapsed: boolean;
  toggleCollapsed: () => void;
}

const BudgetParentCard = (props: BudgetParentCardProps): React.ReactNode => {
  const [isSelected, { toggle, close }] = useDisclosure(false);
  const childrenId = React.useId();

  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { allTransactionCategories } = useTransactionCategories();
  const updateBudgetMutation = useUpdateBudgetMutation();
  const deleteBudgetMutation = useDeleteBudgetMutation();

  const isIncome = areStringsEqual(
    props.categoryTree.categoryType,
    CategoryTypes.Income,
  );
  const categoryIcon = getCategoryIcon(
    props.categoryTree.value,
    allTransactionCategories,
  );
  const limit =
    props.categoryToLimitsMap.get(props.categoryTree.value.toLowerCase()) ?? 0;
  const amount =
    props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0;
  const projectedAmount =
    amount +
    (props.categoryToRecurringForecastTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0);
  const budgets =
    props.categoryToBudgetsMap.get(props.categoryTree.value.toLowerCase()) ??
    [];
  const id =
    budgets.length === 1 && props.selectedDate ? (budgets[0]?.id ?? "") : "";
  // Rollover is on for either the parent or its children, never both. When only children roll
  // over, the parent's rollover is the sum of theirs.
  const rolloverStartMonth = budgets[0]?.rolloverStartMonth ?? null;
  const parentRollsOver = budgets.some(
    (budget) => budget.rolloverStartMonth !== null,
  );
  const hasRolloverChildren = props.categoryTree.subCategories.some(
    (subCategory) =>
      (
        props.categoryToBudgetsMap.get(subCategory.value.toLowerCase()) ?? []
      ).some((budget) => budget.rolloverStartMonth !== null),
  );
  const rollover =
    props.categoryToRolloverMap.get(props.categoryTree.value.toLowerCase()) ??
    0;
  const availableLimit = limit + rollover;

  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const percentComplete = roundAwayFromZero(
    (((props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0) *
      (isIncome ? 1 : -1)) /
      availableLimit) *
      100,
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
  const projectedProgressValue = Math.max(
    0,
    projectedPercentComplete - actualProgressValue,
  );
  const handleEdit = (
    newLimit?: number | string,
    newRolloverStartMonth?: string | null,
  ) => {
    if (newLimit === "") {
      return;
    }
    if (id.length === 0) {
      return;
    }
    updateBudgetMutation.mutate({
      id,
      limit: Number(newLimit),
      rolloverStartMonth:
        newRolloverStartMonth === undefined
          ? rolloverStartMonth
          : newRolloverStartMonth,
    });
  };

  const childLimitsTotal = props.categoryTree.subCategories.reduce(
    (acc, subCategory) => {
      const limit =
        props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ?? 0;
      return acc + limit;
    },
    0,
  );

  const buildChildren = (): React.ReactNode[] => {
    const budgetedChildCards: React.ReactNode[] = [];
    const unbudgetedChildCards: React.ReactNode[] = [];

    props.categoryTree.subCategories.forEach((subCategory) => {
      if (
        props.categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase())
      ) {
        const budgets =
          props.categoryToBudgetsMap.get(
            subCategory.value.toLocaleLowerCase(),
          ) ?? [];
        const budgetId =
          budgets.length === 1 && props.selectedDate
            ? (budgets[0]?.id ?? "")
            : "";
        budgetedChildCards.push(
          <BudgetChildCard
            key={subCategory.value}
            id={budgetId}
            categoryValue={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            projectedAmount={
              (props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0) +
              (props.categoryToRecurringForecastTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0)
            }
            limit={
              props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ??
              0
            }
            rollover={
              props.categoryToRolloverMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            rolloverStartMonth={budgets[0]?.rolloverStartMonth ?? null}
            rolloverDisabledReason={
              parentRollsOver
                ? t("rollover_on_for_category", {
                    category: props.categoryTree.value,
                  })
                : undefined
            }
            isIncome={isIncome}
            icon={getCategoryIcon(subCategory.value, allTransactionCategories)}
            selectedDate={props.selectedDate ?? dayjs().toDate()}
            openDetails={props.openDetails}
          />,
        );
      } else if (
        props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase(),
        )
      ) {
        const amount =
          props.categoryToTransactionsTotalMap.get(
            subCategory.value.toLowerCase(),
          ) ?? 0;
        if (roundAwayFromZero(amount) !== 0) {
          unbudgetedChildCards.push(
            <UnbudgetChildCard
              key={subCategory.value}
              category={subCategory.value}
              amount={amount}
              selectedDate={props.selectedDate}
              isIncome={isIncome}
              icon={getCategoryIcon(
                subCategory.value,
                allTransactionCategories,
              )}
              openDetails={props.openDetails}
            />,
          );
        }
      }
    });

    return [...budgetedChildCards, ...unbudgetedChildCards];
  };

  const childCards = buildChildren();

  const getElementForLimit = () => {
    if (isSelected) {
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

  return (
    <Card p={0} w="100%" elevation={1}>
      <Box
        m="0.25rem"
        p="0.25rem 0.5rem"
        className={`${classes.header} ${hoverClasses.hoverable} ${hoverClasses.outline}`}
        data-hover-effect="true"
        onClick={() => {
          if (id.length > 0) {
            props.openDetails(props.categoryTree.value, props.selectedDate);
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
              gap={0}
            >
              <Group gap="0.25rem" align="center">
                {childCards.length > 0 && (
                  <ActionIcon
                    variant="ghost"
                    color="primary"
                    size="compact-xs"
                    aria-label={t("toggle_budget_category", {
                      category: props.categoryTree.value,
                    })}
                    aria-expanded={!props.isCollapsed}
                    aria-controls={!props.isCollapsed ? childrenId : undefined}
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
                {isSelected && (
                  <CategoryIconPicker
                    category={props.categoryTree.value}
                    icon={categoryIcon}
                  />
                )}
                <PrimaryHeading className={classes.title}>
                  {!isSelected && categoryIcon.length > 0
                    ? `${categoryIcon} `
                    : ""}
                  {props.categoryTree.value}
                </PrimaryHeading>
                <ActionIcon
                  variant="ghost"
                  color="primary"
                  size="compact-xs"
                  selected={isSelected}
                  onClick={(e) => {
                    e.stopPropagation();
                    if (id.length > 0) {
                      newLimitField.setValue(limit);
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
                      amount={amount}
                      disableStatusColor
                      size="md"
                      isSensitive={isPrivacyModeEnabled}
                      locale={intlLocale}
                      currency={preferredCurrency}
                      decimalPlaces={0}
                      signDisplay={SignDisplay.Auto}
                      invertSign={!isIncome}
                      className={classes.text}
                      key="amount"
                    />,
                    <DimmedText size="sm" key="of" elevation={1} />,
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
              type={isIncome ? "income" : "expense"}
              warningThreshold={budgetWarningThreshold}
              ariaLabel={props.categoryTree.value}
            />
            <BudgetMetrics
              amount={amount}
              projectedAmount={projectedAmount}
              limit={availableLimit}
              rollover={
                parentRollsOver || hasRolloverChildren ? rollover : undefined
              }
              isIncome={isIncome}
            />
          </Stack>
          {isSelected && (
            <Flex
              style={{ alignSelf: "stretch" }}
              gap="0.25rem"
              onClick={(e) => e.stopPropagation()}
            >
              <RolloverButton
                category={props.categoryTree.value}
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
                <MantinePopover.Target>
                  <ActionIcon
                    variant="filled"
                    color="error"
                    size="compact-sm"
                    h="100%"
                  >
                    <TrashIcon size="1rem" />
                  </ActionIcon>
                </MantinePopover.Target>
                <MantinePopover.Dropdown p="0.5rem" maw={200}>
                  <Stack gap={5}>
                    <PrimaryText size="sm" elevation={1}>
                      {t("confirm_delete_budget_message")}
                    </PrimaryText>
                    <DimmedText size="xs" elevation={1}>
                      {t("all_children_will_also_be_deleted")}
                    </DimmedText>
                    <Button
                      variant="filled"
                      color="error"
                      size="compact-xs"
                      onClick={() => {
                        deleteBudgetMutation.mutate(id);
                        close();
                      }}
                    >
                      {t("delete")}
                    </Button>
                  </Stack>
                </MantinePopover.Dropdown>
              </Popover>
            </Flex>
          )}
        </Group>
      </Box>
      {childCards.length > 0 && !props.isCollapsed && (
        <>
          <Divider w="100%" size="sm" elevation={0} />
          <Stack
            id={childrenId}
            role="region"
            aria-label={props.categoryTree.value}
            gap={0}
          >
            {childCards.map((childCard, index) => (
              <React.Fragment key={index}>
                {index > 0 && <Divider w="100%" size="xs" elevation={0} />}
                {childCard}
              </React.Fragment>
            ))}
          </Stack>
        </>
      )}
    </Card>
  );
};

export default BudgetParentCard;
