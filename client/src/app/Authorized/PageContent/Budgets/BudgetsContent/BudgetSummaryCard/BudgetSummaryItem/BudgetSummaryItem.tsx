import { StatusColorType } from "~/helpers/budgets";
import { SignDisplay } from "~/helpers/currency";
import { Flex, Group, Stack } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { Trans } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { roundAwayFromZero } from "~/helpers/utils";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { AmountText, Progress } from "@teelur/budget-board-ui";

interface BudgetSummaryItemProps {
  label: string;
  amount: number;
  projectedAmount?: number;
  total?: number;
  budgetValueType: StatusColorType;
  hideProgress?: boolean;
  showDivider?: boolean;
}

const BudgetSummaryItem = (props: BudgetSummaryItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const formatAmount = useSensitiveAmountFormatter();
  const formatSensitiveAmount = (amount: number): string =>
    formatAmount(amount, SignDisplay.Auto, undefined, 0);

  const percentComplete = Math.round(
    ((props.amount *
      (props.budgetValueType === StatusColorType.Expense ? -1 : 1)) /
      (props.total ?? 0)) *
      100,
  );

  const invertBudgetSign = props.budgetValueType === StatusColorType.Expense;
  const hasProjection =
    props.projectedAmount !== undefined &&
    roundAwayFromZero(props.projectedAmount - props.amount) !== 0;
  const formattedTotal = formatSensitiveAmount(props.total ?? 0);
  const actualProgressValue = Math.min(100, Math.max(0, percentComplete));
  const projectedPercentComplete =
    props.projectedAmount === undefined || (props.total ?? 0) <= 0
      ? actualProgressValue
      : Math.min(
          100,
          Math.max(
            0,
            roundAwayFromZero(
              ((props.projectedAmount * (invertBudgetSign ? -1 : 1)) /
                (props.total ?? 0)) *
                100,
            ),
          ),
        );
  const projectedProgressValue = Math.max(
    0,
    projectedPercentComplete - actualProgressValue,
  );

  const amountTextProps = {
    amount: props.amount,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold: budgetWarningThreshold,
    size: "md" as const,
    isSensitive: isPrivacyModeEnabled,
    locale: intlLocale,
    currency: preferredCurrency,
    decimalPlaces: 0,
    signDisplay: SignDisplay.Auto,
    invertSign: invertBudgetSign,
  };
  const projectedAmountTextProps = {
    amount: props.projectedAmount ?? 0,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold: budgetWarningThreshold,
    size: "sm" as const,
    isSensitive: isPrivacyModeEnabled,
    locale: intlLocale,
    currency: preferredCurrency,
    decimalPlaces: 0,
    signDisplay: SignDisplay.Auto,
    invertSign: invertBudgetSign,
  };

  const getAmountText = () => {
    if (props.total) {
      return (
        <Trans
          i18nKey="x_of_y"
          values={{ total: formattedTotal }}
          components={[
            <AmountText {...amountTextProps} key="amount" />,
            <DimmedText size="sm" key="of" />,
            <AmountText
              amount={props.total ?? 0}
              disableStatusColor
              size="md"
              isSensitive={isPrivacyModeEnabled}
              locale={intlLocale}
              currency={preferredCurrency}
              decimalPlaces={0}
              signDisplay={SignDisplay.Auto}
              key="total"
            />,
          ]}
        />
      );
    }

    return <AmountText {...amountTextProps} key="amount" />;
  };

  const getProjectedAmountText = () => {
    if (props.total) {
      return (
        <Trans
          i18nKey="budget_projected_fraction_styled"
          values={{ total: formattedTotal }}
          components={[
            <DimmedText size="xs" key="label" />,
            <AmountText {...projectedAmountTextProps} key="amount" />,
            <DimmedText size="xs" key="of" />,
            <PrimaryText size="sm" key="total" />,
          ]}
        />
      );
    }

    return (
      <>
        <DimmedText size="xs" key="label">
          {t("projected_colon")}
        </DimmedText>
        <AmountText {...projectedAmountTextProps} key="amount" />
      </>
    );
  };

  return (
    <Stack gap={0}>
      <Group gap="0.25rem" justify="space-between" align="center">
        <Flex style={{ flex: "1 1 auto", minWidth: 0 }}>
          <PrimaryText size="md">{props.label}</PrimaryText>
        </Flex>
        <Stack
          gap={0}
          align="flex-end"
          style={{ minWidth: 0, maxWidth: "100%" }}
        >
          <Flex gap="0.25rem" align="baseline">
            {getAmountText()}
          </Flex>
          {hasProjection && (
            <Group gap="0.25rem" align="baseline" style={{ maxWidth: "100%" }}>
              {getProjectedAmountText()}
            </Group>
          )}
        </Stack>
      </Group>
      {!props.hideProgress && (props.total ?? 0) > 0 && (
        <Progress
          amount={props.amount}
          limit={props.total ?? 0}
          label
          type={invertBudgetSign ? "expense" : "income"}
          size="xs"
          sections={
            projectedProgressValue > 0
              ? [
                  {
                    ariaLabel: t("recurring_transactions"),
                    color: "muted",
                    striped: true,
                    animated: true,
                    value: projectedProgressValue,
                  },
                ]
              : []
          }
          warningThreshold={budgetWarningThreshold}
          ariaLabel={props.label}
        />
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
