import { StatusColorType } from "~/helpers/budgets";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Flex, Group, Stack } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import { Trans } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface BudgetSummaryItemProps {
  label: string;
  amount: number;
  total?: number;
  budgetValueType: StatusColorType;
  hideProgress?: boolean;
  showDivider?: boolean;
}

const BudgetSummaryItem = (props: BudgetSummaryItemProps): React.ReactNode => {
  const { intlLocale } = useLocale();
  const { preferredCurrency, budgetWarningThreshold } = useUserSettings();

  const percentComplete = Math.round(
    ((props.amount *
      (props.budgetValueType === StatusColorType.Expense ? -1 : 1)) /
      (props.total ?? 0)) *
      100,
  );

  const signedAmount =
    props.amount * (props.budgetValueType === StatusColorType.Expense ? -1 : 1);

  const formattedAmount = convertNumberToCurrency(
    signedAmount,
    false,
    preferredCurrency,
    SignDisplay.Auto,
    intlLocale,
  );
  const formattedTotal = convertNumberToCurrency(
    props.total ?? 0,
    false,
    preferredCurrency,
    SignDisplay.Auto,
    intlLocale,
  );

  const statusTextProps = {
    amount: props.amount,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold: budgetWarningThreshold,
    size: "md" as const,
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

  return (
    <Stack gap={0}>
      <Group gap="0.25rem" justify="space-between">
        <Flex>
          <PrimaryText size="md">{props.label}</PrimaryText>
        </Flex>
        <Flex gap="0.25rem" align="baseline">
          <Trans
            i18nKey={i18nKey}
            values={transValues}
            components={transComponents}
          />
        </Flex>
      </Group>
      {!props.hideProgress && (props.total ?? 0) > 0 && (
        <Progress
          size={16}
          percentComplete={percentComplete}
          amount={props.amount}
          limit={props.total ?? 0}
          type={
            props.budgetValueType === StatusColorType.Income
              ? ProgressType.Income
              : ProgressType.Expense
          }
          warningThreshold={budgetWarningThreshold}
          elevation={1}
        />
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
