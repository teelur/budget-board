import classes from "./BudgetParentCard.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import { getCurrencySymbol, SignDisplay } from "~/helpers/currency";
import {
  ActionIcon,
  Box,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Popover as MantinePopover,
  Stack,
} from "@mantine/core";
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
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import Popover from "~/components/core/Popover/Popover";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import BudgetMetrics from "./BudgetMetrics/BudgetMetrics";
import { useTranslation, Trans } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";
import { useDeleteBudgetMutation } from "~/hooks/mutations/budgets/useDeleteBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
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
  const { dayjs, thousandsSeparator, decimalSeparator } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const formatAmount = useSensitiveAmountFormatter();
  const formatSensitiveAmount = (amount: number): string =>
    formatAmount(amount, false, SignDisplay.Auto);
  const updateBudgetMutation = useUpdateBudgetMutation();
  const deleteBudgetMutation = useDeleteBudgetMutation();

  const isIncome = areStringsEqual(
    props.categoryTree.categoryType,
    CategoryTypes.Income,
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

  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const percentComplete = roundAwayFromZero(
    (((props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0) *
      (isIncome ? 1 : -1)) /
      limit) *
      100,
  );
  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (id.length === 0) {
      return;
    }
    updateBudgetMutation.mutate({
      id,
      limit: Number(newLimit),
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
            isIncome={isIncome}
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
              openDetails={props.openDetails}
            />,
          );
        }
      }
    });

    return [...budgetedChildCards, ...unbudgetedChildCards];
  };

  const childCards = buildChildren();

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
                    variant="transparent"
                    size="md"
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
                <PrimaryHeading className={classes.title}>
                  {props.categoryTree.value}
                </PrimaryHeading>
                <ActionIcon
                  variant={isSelected ? "outline" : "transparent"}
                  size="md"
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
              <Group gap="0.5rem" justify="flex-end" align="center">
                {isSelected ? (
                  <>
                    <Trans
                      i18nKey="budget_amount_fraction_editable_total_styled"
                      values={{
                        amount: formatSensitiveAmount(
                          amount * (isIncome ? 1 : -1),
                        ),
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
                        min={childLimitsTotal}
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
                        elevation={1}
                      />
                    </Flex>
                  </>
                ) : (
                  <Trans
                    i18nKey="budget_amount_fraction_styled"
                    values={{
                      amount: formatSensitiveAmount(
                        amount * (isIncome ? 1 : -1),
                      ),
                      total: formatSensitiveAmount(limit),
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
            <Group gap="0.25rem" style={{ containerType: "inline-size" }}>
              <Flex style={{ flex: "1 1 auto" }}>
                <Progress
                  size={16}
                  percentComplete={percentComplete}
                  amount={amount}
                  limit={limit}
                  projectedAmount={projectedAmount}
                  type={isIncome ? ProgressType.Income : ProgressType.Expense}
                  warningThreshold={budgetWarningThreshold}
                  elevation={1}
                />
              </Flex>
            </Group>
            <BudgetMetrics
              amount={amount}
              projectedAmount={projectedAmount}
              limit={limit}
              isIncome={isIncome}
              budgetWarningThreshold={budgetWarningThreshold}
              formatAmount={formatSensitiveAmount}
            />
          </Stack>
          {isSelected && (
            <Flex
              style={{ alignSelf: "stretch" }}
              onClick={(e) => e.stopPropagation()}
            >
              <Popover>
                <MantinePopover.Target>
                  <ActionIcon color="var(--button-color-destructive)" h="100%">
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
                      color="var(--button-color-destructive)"
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
