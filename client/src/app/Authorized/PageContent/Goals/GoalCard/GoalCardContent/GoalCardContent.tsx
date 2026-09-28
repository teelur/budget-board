import classes from "./GoalCardContent.module.css";

import { Flex, Group, Stack } from "@mantine/core";
import { ActionIcon, Badge, AmountText } from "@teelur/budget-board-ui";
import React from "react";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { SignDisplay } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { PencilIcon } from "lucide-react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { StatusColorType } from "~/helpers/budgets";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import Progress from "~/components/core/Progress/Progress";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface GoalCardContentProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  toggleIsSelected: () => void;
}

const GoalCardContent = (props: GoalCardContentProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const formatAmount = useSensitiveAmountFormatter();
  const formatSensitiveAmount = (amount: number): string =>
    formatAmount(amount, SignDisplay.Auto, undefined, 0);

  return (
    <Group style={{ containerType: "inline-size" }} wrap="nowrap">
      <Stack w="100%" gap="0.1rem">
        <Flex className={classes.header}>
          <Group align="center" gap={10} wrap="nowrap">
            <PrimaryText size="lg">{props.goal.name}</PrimaryText>
            {props.includeInterest && props.goal.interestRate && (
              <Badge variant="light" color="primary" size="xs">
                {t("interest_rate_apr", {
                  rate: new Intl.NumberFormat(intlLocale, {
                    style: "percent",
                    maximumFractionDigits: 2,
                  }).format(props.goal.interestRate),
                })}
              </Badge>
            )}
            <ActionIcon
              variant="ghost"
              color="primary"
              size="compact-xs"
              onClick={(e) => {
                e.stopPropagation();
                props.toggleIsSelected();
              }}
            >
              <PencilIcon size={16} />
            </ActionIcon>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            <Trans
              i18nKey="x_of_y"
              components={[
                <AmountText
                  amount={
                    sumAccountsTotalBalance(props.goal.accounts) -
                    props.goal.initialAmount
                  }
                  disableStatusColor
                  size="lg"
                  isSensitive={isPrivacyModeEnabled}
                  locale={intlLocale}
                  currency={preferredCurrency}
                  decimalPlaces={0}
                  signDisplay={SignDisplay.Auto}
                  invertSign={false}
                  key="amount"
                />,
                <DimmedText size="md" key="of" />,
                <AmountText
                  amount={getGoalTargetAmount(
                    props.goal.amount,
                    props.goal.initialAmount,
                  )}
                  disableStatusColor
                  size="lg"
                  isSensitive={isPrivacyModeEnabled}
                  locale={intlLocale}
                  currency={preferredCurrency}
                  decimalPlaces={0}
                  signDisplay={SignDisplay.Auto}
                  invertSign={false}
                  key="total"
                />,
              ]}
            />
          </Flex>
        </Flex>
        <Progress
          size={18}
          percentComplete={props.goal.percentComplete}
          amount={0}
          limit={0}
          type={ProgressType.Default}
          elevation={1}
        />
        <Flex className={classes.footer}>
          <Group align="center" gap="0.25rem">
            <DimmedText size="sm" key="label">
              {t("projected_colon")}
            </DimmedText>
            <PrimaryText size="sm" key="date-not-edit">
              {dayjs(props.goal.completeDate).format("MMMM YYYY")}
            </PrimaryText>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            <Trans
              i18nKey="x_of_y_this_month"
              values={{
                total: formatSensitiveAmount(props.goal.monthlyContribution),
              }}
              components={[
                <AmountText
                  amount={props.goal.monthlyContributionProgress}
                  size="md"
                  total={props.goal.monthlyContribution}
                  type={StatusColorType.Target}
                  isSensitive={isPrivacyModeEnabled}
                  locale={intlLocale}
                  currency={preferredCurrency}
                  decimalPlaces={0}
                  key="amount"
                />,
                <DimmedText size="sm" key="of" />,
                <AmountText
                  amount={props.goal.monthlyContribution}
                  size="md"
                  disableStatusColor
                  isSensitive={isPrivacyModeEnabled}
                  locale={intlLocale}
                  currency={preferredCurrency}
                  decimalPlaces={0}
                  key="total-not-edit"
                />,
              ]}
            />
          </Flex>
        </Flex>
      </Stack>
    </Group>
  );
};

export default GoalCardContent;
