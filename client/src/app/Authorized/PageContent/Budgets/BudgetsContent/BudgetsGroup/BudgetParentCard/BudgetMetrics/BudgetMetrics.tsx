import classes from "./BudgetMetrics.module.css";

import { Box, Group } from "@mantine/core";
import React from "react";
import { Trans } from "react-i18next";
import { StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";

interface BudgetMetricsProps {
  amount: number;
  projectedAmount: number;
  limit: number;
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

  return (
    <Group className={classes.metrics} gap={0} align="baseline" wrap="wrap">
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
                  amount={props.projectedAmount}
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
        </Group>
      )}
      <Box className={`${classes.metric} ${classes.currentMetric}`}>
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
    </Group>
  );
};

export default BudgetMetrics;
