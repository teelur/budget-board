import classes from "./BudgetMetrics.module.css";
import { useTranslation } from "react-i18next";
import { Box, Group, Stack, Tooltip } from "@mantine/core";
import { AmountText } from "@teelur/budget-board-ui";
import React from "react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { Trans } from "react-i18next";
import { StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";

interface BudgetMetricsProps {
  amount: number;
  projectedAmount: number;
  /** Already includes any rollover carried in from prior months. */
  limit: number;
  /** Undefined when rollover is disabled. */
  rollover?: number;
  isIncome: boolean;
}

const BudgetMetrics = (props: BudgetMetricsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  const budgetSign = props.isIncome ? 1 : -1;
  const forecastAmount = props.projectedAmount - props.amount;
  const hasProjection = roundAwayFromZero(forecastAmount) !== 0;
  const actualRemaining = roundAwayFromZero(
    props.limit - props.amount * budgetSign,
  );
  const rollover = roundAwayFromZero(props.rollover ?? 0);
  const hasRollover = props.rollover !== undefined;

  return (
    <Group className={classes.metrics} gap={0} align="baseline" wrap="wrap">
      {rollover !== 0 && (
        <Group gap="0.25rem" className={classes.metric}>
          <DimmedText className={classes.inlineText} size="sm" elevation={1}>
            {t("rolled_over_colon")}
          </DimmedText>
          <AmountText
            amount={rollover}
            size="sm"
            disableStatusColor
            isSensitive={isPrivacyModeEnabled}
            locale={intlLocale}
            currency={preferredCurrency}
            decimalPlaces={0}
            className={classes.inlineText}
          />
        </Group>
      )}
      {hasProjection && (
        <Group
          className={classes.forecastGroup}
          gap="1rem"
          align="baseline"
          wrap="nowrap"
        >
          <Group gap="0.25rem">
            <DimmedText
              className={classes.inlineText}
              size="sm"
              key="label"
              elevation={1}
            >
              {t("projected_colon")}
            </DimmedText>
            <AmountText
              amount={props.projectedAmount}
              size="sm"
              disableStatusColor
              isSensitive={isPrivacyModeEnabled}
              locale={intlLocale}
              currency={preferredCurrency}
              decimalPlaces={0}
              invertSign={!props.isIncome}
              className={classes.inlineText}
              key="amount"
            />
          </Group>
          <Box className={classes.metric}>
            <Trans
              i18nKey="budget_left_after_predictions_styled"
              components={[
                <AmountText
                  amount={props.projectedAmount}
                  size="sm"
                  total={props.limit}
                  type={
                    props.isIncome
                      ? StatusColorType.Income
                      : StatusColorType.Expense
                  }
                  warningThreshold={budgetWarningThreshold}
                  isSensitive={isPrivacyModeEnabled}
                  className={classes.inlineText}
                  key="amount"
                >
                  {convertNumberToCurrency(
                    roundAwayFromZero(
                      props.limit - props.projectedAmount * budgetSign,
                    ),
                    0,
                    preferredCurrency,
                    SignDisplay.Auto,
                    intlLocale,
                  )}
                </AmountText>,
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
            rollover={rollover}
            used={props.amount * budgetSign}
            remaining={actualRemaining}
            isIncome={props.isIncome}
          />
        }
        disabled={!hasRollover}
        events={{ hover: true, focus: true, touch: true }}
        position="bottom-end"
        classNames={{ tooltip: classes.breakdownTooltip }}
      >
        <Box
          className={`${classes.metric} ${classes.currentMetric} ${
            hasRollover ? classes.hasBreakdown : ""
          }`}
          tabIndex={hasRollover ? 0 : undefined}
        >
          <Trans
            i18nKey="budget_left_styled"
            components={[
              <AmountText
                amount={props.amount}
                size="md"
                total={props.limit}
                type={
                  props.isIncome
                    ? StatusColorType.Income
                    : StatusColorType.Expense
                }
                isSensitive={isPrivacyModeEnabled}
                locale={intlLocale}
                currency={preferredCurrency}
                decimalPlaces={0}
                warningThreshold={budgetWarningThreshold}
                className={classes.inlineText}
                key="amount"
              >
                {convertNumberToCurrency(
                  actualRemaining,
                  0,
                  preferredCurrency,
                  SignDisplay.Auto,
                  intlLocale,
                )}
              </AmountText>,
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
}

const RolloverBreakdown = (props: RolloverBreakdownProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  const rows: [string, number][] = [
    [t("budget_breakdown_this_month"), props.limit],
    [t("budget_breakdown_rolled_over"), props.rollover],
    [
      props.isIncome
        ? t("budget_breakdown_received")
        : t("budget_breakdown_spent"),
      -props.used,
    ],
  ];

  const renderAmount = (amount: number): React.ReactNode => (
    <AmountText
      amount={amount}
      size="sm"
      type={StatusColorType.Total}
      isSensitive={isPrivacyModeEnabled}
      locale={intlLocale}
      currency={preferredCurrency}
      decimalPlaces={0}
      signDisplay={SignDisplay.Auto}
    />
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
