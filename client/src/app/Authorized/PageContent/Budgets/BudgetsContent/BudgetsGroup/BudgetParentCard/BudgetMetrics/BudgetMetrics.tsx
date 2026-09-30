import classes from "./BudgetMetrics.module.css";

import { Box, Group, Stack, Tooltip } from "@mantine/core";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import { StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";

interface BudgetMetricsProps {
  amount: number;
  projectedAmount: number;
  /** Already includes any rollover carried in from prior months. */
  limit: number;
  rollover?: number;
  isIncome: boolean;
  budgetWarningThreshold: number;
  formatAmount: (amount: number) => string;
}

const BudgetMetrics = (props: BudgetMetricsProps): React.ReactNode => {
  const budgetSign = props.isIncome ? 1 : -1;
  const forecastAmount = props.projectedAmount - props.amount;
  const hasProjection = roundAwayFromZero(forecastAmount) !== 0;
  const actualRemaining = roundAwayFromZero(
    props.limit - props.amount * budgetSign,
  );
  const projectedRemaining = roundAwayFromZero(
    props.limit - props.projectedAmount * budgetSign,
  );
  const statusType = props.isIncome
    ? StatusColorType.Income
    : StatusColorType.Expense;
  const rollover = roundAwayFromZero(props.rollover ?? 0);

  return (
    <Group className={classes.metrics} gap={0} align="baseline" wrap="wrap">
      {rollover !== 0 && (
        <Box className={classes.metric}>
          <Trans
            i18nKey="budget_rolled_over_styled"
            values={{ amount: props.formatAmount(rollover) }}
            components={[
              <DimmedText
                className={classes.inlineText}
                size="sm"
                key="label"
                elevation={1}
              />,
              <PrimaryText
                className={classes.inlineText}
                size="sm"
                key="amount"
                elevation={1}
              />,
            ]}
          />
        </Box>
      )}
      {hasProjection && (
        <Group
          className={classes.forecastGroup}
          gap="1rem"
          align="baseline"
          wrap="nowrap"
        >
          <Box className={classes.metric}>
            <Trans
              i18nKey="budget_projected_styled"
              values={{
                amount: props.formatAmount(props.projectedAmount * budgetSign),
              }}
              components={[
                <DimmedText
                  className={classes.inlineText}
                  size="sm"
                  key="label"
                  elevation={1}
                />,
                <PrimaryText
                  className={classes.inlineText}
                  size="sm"
                  key="amount"
                  elevation={1}
                />,
              ]}
            />
          </Box>
          <Box className={classes.metric}>
            <Trans
              i18nKey="budget_left_after_predictions_styled"
              values={{ amount: props.formatAmount(projectedRemaining) }}
              components={[
                <StatusText
                  size="sm"
                  amount={props.projectedAmount}
                  total={props.limit}
                  type={statusType}
                  warningThreshold={props.budgetWarningThreshold}
                  className={classes.inlineText}
                  key="amount"
                />,
                <DimmedText
                  className={classes.inlineText}
                  size="sm"
                  key="label"
                  elevation={1}
                />,
              ]}
            />
          </Box>
        </Group>
      )}
      <Tooltip
        label={
          <RolloverBreakdown
            limit={props.limit - (props.rollover ?? 0)}
            rollover={props.rollover ?? 0}
            used={props.amount * budgetSign}
            remaining={actualRemaining}
            isIncome={props.isIncome}
            formatAmount={props.formatAmount}
          />
        }
        disabled={props.rollover === undefined}
        events={{ hover: true, focus: true, touch: true }}
        position="bottom-end"
        classNames={{ tooltip: classes.breakdownTooltip }}
      >
        <Box
          className={`${classes.metric} ${classes.currentMetric} ${
            props.rollover !== undefined ? classes.hasBreakdown : ""
          }`}
          tabIndex={props.rollover !== undefined ? 0 : undefined}
        >
          <Trans
            i18nKey="budget_left_styled"
            values={{ amount: props.formatAmount(actualRemaining) }}
            components={[
              <StatusText
                amount={props.amount}
                total={props.limit}
                type={statusType}
                warningThreshold={props.budgetWarningThreshold}
                className={`${classes.heroAmount} ${classes.inlineText}`}
                key="amount"
              />,
              <DimmedText
                className={classes.inlineText}
                size="sm"
                key="label"
                elevation={1}
              />,
            ]}
          />
        </Box>
      </Tooltip>
    </Group>
  );
};

interface RolloverBreakdownProps {
  limit: number;
  rollover: number;
  used: number;
  remaining: number;
  isIncome: boolean;
  formatAmount: (amount: number) => string;
}

const RolloverBreakdown = (props: RolloverBreakdownProps): React.ReactNode => {
  const { t } = useTranslation();

  const rows: [string, number][] = [
    [t("budget_breakdown_this_month"), props.limit],
    [t("budget_breakdown_rolled_over"), roundAwayFromZero(props.rollover)],
    [
      props.isIncome
        ? t("budget_breakdown_received")
        : t("budget_breakdown_spent"),
      -props.used,
    ],
  ];

  const renderAmount = (amount: number): React.ReactNode => (
    <StatusText
      size="sm"
      amount={amount}
      type={StatusColorType.Total}
    >
      {props.formatAmount(amount)}
    </StatusText>
  );

  return (
    <Stack gap={2}>
      {rows.map(([label, amount]) => (
        <Group key={label} justify="space-between" gap="1rem" wrap="nowrap">
          <span>{label}</span>
          {renderAmount(amount)}
        </Group>
      ))}
      <Group
        justify="space-between"
        gap="1rem"
        wrap="nowrap"
        className={classes.breakdownTotal}
      >
        <span>{t("budget_breakdown_left")}</span>
        {renderAmount(props.remaining)}
      </Group>
    </Stack>
  );
};

export default BudgetMetrics;
