import classes from "./GoalCardContent.module.css";

import { ActionIcon, Badge, Flex, Group, Stack } from "@mantine/core";
import React from "react";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { maskedAmountText } from "~/helpers/privacy";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { PencilIcon } from "lucide-react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { StatusColorType } from "~/helpers/budgets";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import Progress from "~/components/core/Progress/Progress";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
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

  const formatSensitiveAmount = (amount: number): string =>
    isPrivacyModeEnabled
      ? maskedAmountText
      : convertNumberToCurrency(
          amount,
          false,
          preferredCurrency,
          SignDisplay.Auto,
          intlLocale,
        );

  return (
    <Group style={{ containerType: "inline-size" }} wrap="nowrap">
      <Stack w="100%" gap="0.1rem">
        <Flex className={classes.header}>
          <Group align="center" gap={10} wrap="nowrap">
            <PrimaryText size="lg">{props.goal.name}</PrimaryText>
            {props.includeInterest && props.goal.interestRate && (
              <Badge variant="light" flex="0 0 auto">
                {t("interest_rate_apr", {
                  rate: new Intl.NumberFormat(intlLocale, {
                    style: "percent",
                    maximumFractionDigits: 2,
                  }).format(props.goal.interestRate),
                })}
              </Badge>
            )}
            <ActionIcon
              variant="transparent"
              size="md"
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
              i18nKey="budget_amount_fraction_styled"
              values={{
                amount: formatSensitiveAmount(
                  sumAccountsTotalBalance(props.goal.accounts) -
                    props.goal.initialAmount,
                ),
                total: formatSensitiveAmount(
                  getGoalTargetAmount(
                    props.goal.amount,
                    props.goal.initialAmount,
                  ),
                ),
              }}
              components={[
                <PrimaryText size="lg" key="amount" />,
                <DimmedText size="md" key="of" />,
                <PrimaryText size="lg" key="total" />,
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
          <Group align="center" gap="sm">
            <Flex align="center" gap="0.25rem">
              <Trans
                i18nKey="budget_projected_styled"
                values={{
                  amount: dayjs(props.goal.completeDate).format("MMMM YYYY"),
                }}
                components={[
                  <DimmedText size="sm" key="label" />,
                  <PrimaryText size="sm" key="date-not-edit" />,
                ]}
              />
            </Flex>
          </Group>
          <Flex justify="flex-end" align="center" gap="0.25rem">
            <Trans
              i18nKey="budget_monthly_amount_fraction_styled"
              values={{
                amount: formatSensitiveAmount(
                  props.goal.monthlyContributionProgress,
                ),
                total: formatSensitiveAmount(
                  props.goal.monthlyContribution,
                ),
              }}
              components={[
                <StatusText
                  amount={props.goal.monthlyContributionProgress}
                  total={props.goal.monthlyContribution}
                  type={StatusColorType.Target}
                  size="md"
                  key="amount"
                />,
                <DimmedText size="sm" key="of" />,
                <PrimaryText size="md" key="total-not-edit" />,
              ]}
            />
          </Flex>
        </Flex>
      </Stack>
    </Group>
  );
};

export default GoalCardContent;
