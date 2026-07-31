import { Flex, Skeleton, Stack, Text } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import {
  buildDataRequirements,
  hasCurrencyMetric,
  parseTemplate,
  resolveTemplate,
  MetricDataContext,
} from "~/helpers/metricWidget";
import { maskedAmountText } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import MetricWidgetSettings from "./MetricWidgetSettings/MetricWidgetSettings";
import classes from "./MetricWidget.module.css";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

interface MetricWidgetProps {
  widget: IWidgetSettingsResponse;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const MetricWidget = ({
  widget,
  settingsOpened,
  onSettingsClose,
}: MetricWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { preferredCurrency } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { intlLocale, dayjs } = useLocale();
  const { allAccountTypes, isPending: accountTypesPending } = useAccountTypes();
  const widgetSettingsQuery = useWidgetSettingsQuery();

  // The widget configuration is stored as a JSON string in the database.
  const { configTitle, configValue, configLabel } = React.useMemo(() => {
    if (!widget?.configuration)
      return {
        configTitle: undefined,
        configValue: undefined,
        configLabel: undefined,
      };

    try {
      const parsed = JSON.parse(widget.configuration) as {
        title?: string;
        value?: string;
        label?: string;
      };
      return {
        configTitle: parsed.title,
        configValue: parsed.value,
        configLabel: parsed.label,
      };
    } catch {
      return {
        configTitle: undefined,
        configValue: undefined,
        configLabel: undefined,
      };
    }
  }, [widgetSettingsQuery.data, widget]);

  const parsedValueTokens = React.useMemo(
    () => parseTemplate(configValue ?? ""),
    [configValue],
  );
  const parsedLabelTokens = React.useMemo(
    () => parseTemplate(configLabel ?? ""),
    [configLabel],
  );

  const requirements = React.useMemo(
    () => buildDataRequirements(parsedValueTokens, parsedLabelTokens),
    [parsedValueTokens, parsedLabelTokens],
  );

  const transactionMonthQueries = useTransactionsQuery({
    selectedDates: requirements.transactionMonths.map((date) => ({
      month: dayjs(date).month() + 1,
      year: dayjs(date).year(),
    })),
    enabled:
      requirements.needsTransactions && !requirements.needsAllTimeTransactions,
  });

  const allTimeTransactionsQuery = useTransactionsQuery({
    enabled:
      requirements.needsTransactions && requirements.needsAllTimeTransactions,
  });

  const budgetQueries = useBudgetsQuery({
    months: requirements.budgetMonths,
    enabled: requirements.needsBudgets,
  });

  const goalsQuery = useGoalsQuery({
    includeInterest: false,
    enabled: requirements.needsGoals,
  });

  const accountsQuery = useAccountsQuery({
    enabled: requirements.needsAccounts,
  });

  const isPending =
    widgetSettingsQuery.isPending ||
    (requirements.needsTransactions &&
      (requirements.needsAllTimeTransactions
        ? allTimeTransactionsQuery.isPending
        : transactionMonthQueries.isPending)) ||
    (requirements.needsBudgets && budgetQueries.isPending) ||
    (requirements.needsGoals && goalsQuery.isPending) ||
    (requirements.needsAccounts &&
      (accountsQuery.isPending || accountTypesPending));

  const metricDataContext: MetricDataContext = React.useMemo(
    () => ({
      transactions: requirements.needsAllTimeTransactions
        ? (allTimeTransactionsQuery.data ?? [])
        : (transactionMonthQueries.data ?? []),
      budgets: budgetQueries.data,
      goals: goalsQuery.data ?? [],
      accounts: accountsQuery.data ?? [],
      accountTypes: allAccountTypes ?? [],
      preferredCurrency,
      intlLocale,
    }),
    [
      requirements.needsAllTimeTransactions,
      allTimeTransactionsQuery.data,
      transactionMonthQueries.data,
      budgetQueries.data,
      goalsQuery.data,
      accountsQuery.data,
      allAccountTypes,
      preferredCurrency,
      intlLocale,
    ],
  );

  const titleText = React.useMemo(() => {
    return configTitle || t("metric");
  }, [configTitle, t]);

  const valueText = React.useMemo(() => {
    if (configValue && !isPending) {
      if (isPrivacyModeEnabled && hasCurrencyMetric(parsedValueTokens)) {
        return maskedAmountText;
      }
      return resolveTemplate(parsedValueTokens, metricDataContext);
    }
    return null;
  }, [
    configValue,
    isPending,
    isPrivacyModeEnabled,
    parsedValueTokens,
    metricDataContext,
  ]);

  const labelText = React.useMemo(() => {
    if (configLabel && !isPending) {
      if (isPrivacyModeEnabled && hasCurrencyMetric(parsedLabelTokens)) {
        return maskedAmountText;
      }
      return resolveTemplate(parsedLabelTokens, metricDataContext);
    }
    return null;
  }, [
    configLabel,
    isPending,
    isPrivacyModeEnabled,
    parsedLabelTokens,
    metricDataContext,
  ]);

  const renderContent = () => {
    if (isPending) {
      return (
        <Flex h="100%" w="100%" p="0.5rem">
          <Skeleton flex={1} radius="md" />
        </Flex>
      );
    }

    if (!configValue) {
      return (
        <WidgetErrorMessage messageKey="metric_widget_no_value_expression" />
      );
    }

    return (
      <Stack
        h="100%"
        w="100%"
        align="center"
        justify="center"
        p="1rem"
        gap="0.25rem"
      >
        <Text className={classes.value} ta="center">
          {valueText}
        </Text>
        {labelText && (
          <DimmedText size="sm" ta="center">
            {labelText}
          </DimmedText>
        )}
      </Stack>
    );
  };

  return (
    <SplitCard
      w="100%"
      h="100%"
      style={{ containerType: "inline-size" }}
      border={BorderThickness.Thick}
      header={
        <PrimaryHeading order={4} className={classes.title}>
          {titleText}
        </PrimaryHeading>
      }
      elevation={1}
    >
      {renderContent()}
      {settingsOpened !== undefined && onSettingsClose && (
        <MetricWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default MetricWidget;
