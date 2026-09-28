import classes from "./BudgetMetrics.module.css";
import { useTranslation } from "react-i18next";
import { Box, Group } from "@mantine/core";
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
  limit: number;
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

  return (
    <Group className={classes.metrics} gap={0} align="baseline" wrap="wrap">
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
              className={classes.inlineText}
              key="amount"
            />
          </Group>
          <Box className={classes.metric}>
            <Trans
              i18nKey="budget_left_after_predictions_styled"
              components={[
                <AmountText
                  amount={roundAwayFromZero(
                    props.limit - props.projectedAmount * budgetSign,
                  )}
                  size="sm"
                  total={props.limit}
                  type={
                    props.isIncome
                      ? StatusColorType.Income
                      : StatusColorType.Expense
                  }
                  warningThreshold={budgetWarningThreshold}
                  isSensitive={isPrivacyModeEnabled}
                  locale={intlLocale}
                  currency={preferredCurrency}
                  decimalPlaces={0}
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
      <Box className={`${classes.metric} ${classes.currentMetric}`}>
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
                roundAwayFromZero(props.limit - props.amount * budgetSign),
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
  );
};

export default BudgetMetrics;
