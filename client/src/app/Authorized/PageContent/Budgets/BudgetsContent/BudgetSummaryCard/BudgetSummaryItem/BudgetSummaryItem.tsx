import { StatusColorType } from "~/helpers/budgets";
import { SignDisplay } from "~/helpers/currency";
import { Flex, Group, Stack } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import { Trans } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { roundAwayFromZero } from "~/helpers/utils";

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
  const { budgetWarningThreshold } = useUserSettings();
  const formatAmount = useSensitiveAmountFormatter();
  const formatSensitiveAmount = (amount: number): string =>
    formatAmount(amount, false, SignDisplay.Auto);

  const percentComplete = Math.round(
    ((props.amount *
      (props.budgetValueType === StatusColorType.Expense ? -1 : 1)) /
      (props.total ?? 0)) *
      100,
  );

  const signedAmount =
    props.amount * (props.budgetValueType === StatusColorType.Expense ? -1 : 1);
  const hasProjection =
    props.projectedAmount !== undefined &&
    roundAwayFromZero(props.projectedAmount - props.amount) !== 0;
  const signedProjectedAmount =
    (props.projectedAmount ?? 0) *
    (props.budgetValueType === StatusColorType.Expense ? -1 : 1);

  const formattedAmount = formatSensitiveAmount(signedAmount);
  const formattedProjectedAmount = formatSensitiveAmount(signedProjectedAmount);
  const formattedTotal = formatSensitiveAmount(props.total ?? 0);

  const statusTextProps = {
    amount: props.amount,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold: budgetWarningThreshold,
    size: "md" as const,
  };
  const projectedStatusTextProps = {
    amount: props.projectedAmount ?? 0,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold: budgetWarningThreshold,
    size: "sm" as const,
  };

  const i18nKey = props.total
    ? "budget_amount_fraction_styled"
    : "budget_amount_fraction_no_total_styled";

  const transValues = props.total
    ? { amount: formattedAmount, total: formattedTotal }
    : { amount: formattedAmount };

  const transComponents = props.total
    ? [
        <StatusText {...statusTextProps} key="amount" />,
        <DimmedText size="sm" key="of" />,
        <PrimaryText size="md" key="total" />,
      ]
    : [<StatusText {...statusTextProps} key="amount" />];
  const projectedI18nKey = props.total
    ? "budget_projected_fraction_styled"
    : "budget_projected_styled";
  const projectedTransValues = props.total
    ? { amount: formattedProjectedAmount, total: formattedTotal }
    : { amount: formattedProjectedAmount };
  const projectedTransComponents = props.total
    ? [
        <DimmedText size="xs" key="label" />,
        <StatusText {...projectedStatusTextProps} key="amount" />,
        <DimmedText size="xs" key="of" />,
        <PrimaryText size="sm" key="total" />,
      ]
    : [
        <DimmedText size="xs" key="label" />,
        <StatusText {...projectedStatusTextProps} key="amount" />,
      ];

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
            <Trans
              i18nKey={i18nKey}
              values={transValues}
              components={transComponents}
            />
          </Flex>
          {hasProjection && (
            <Flex gap="0.25rem" align="baseline" style={{ maxWidth: "100%" }}>
              <Trans
                i18nKey={projectedI18nKey}
                values={projectedTransValues}
                components={projectedTransComponents}
              />
            </Flex>
          )}
        </Stack>
      </Group>
      {!props.hideProgress && (props.total ?? 0) > 0 && (
        <Group gap="0.5rem" align="center">
          <Flex style={{ flex: "1 1 auto", minWidth: 0 }}>
            <Progress
              size={8}
              percentComplete={percentComplete}
              amount={props.amount}
              limit={props.total ?? 0}
              projectedAmount={props.projectedAmount}
              type={
                props.budgetValueType === StatusColorType.Income
                  ? ProgressType.Income
                  : ProgressType.Expense
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
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
