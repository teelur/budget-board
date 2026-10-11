import { Flex, Skeleton, Stack } from "@mantine/core";
import { BodyText, Card, HeadingText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import {
  buildDataRequirements,
  hasCurrencyMetric,
  parseTemplate,
  resolveTemplate,
  MetricDataContext,
} from "~/helpers/metricWidget";
import { formatSensitiveText } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import MetricWidgetSettings from "./MetricWidgetSettings/MetricWidgetSettings";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

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
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { intlLocale, dayjs } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { allAccountTypes, isPending: accountTypesPending } = useAccountTypes();
  const { getCategoryType, isPending: transactionCategoriesPending } =
    useTransactionCategories();
  const widgetSettingsQuery = useWidgetSettingsQuery();

  // The widget configuration is stored as a JSON string in the database.
  const { configTitle, configValue, configLabel } = React.useMemo(() => {
    if (!widget?.configuration) {
      return {
        configTitle: undefined,
        configValue: undefined,
        configLabel: undefined,
      };
    }

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
    (requirements.needsTransactions && transactionCategoriesPending) ||
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
      getCategoryType,
      preferredCurrency,
      decimalPlaces,
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
      getCategoryType,
      preferredCurrency,
      decimalPlaces,
      intlLocale,
    ],
  );

  const titleText = React.useMemo(() => {
    return configTitle || t("metric");
  }, [configTitle, t]);

  const valueText = React.useMemo(() => {
    if (configValue && !isPending) {
      const resolvedValue = resolveTemplate(
        parsedValueTokens,
        metricDataContext,
      );
      if (hasCurrencyMetric(parsedValueTokens)) {
        return formatSensitiveText(resolvedValue, isPrivacyModeEnabled);
      }
      return resolvedValue;
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
      const resolvedLabel = resolveTemplate(
        parsedLabelTokens,
        metricDataContext,
      );
      if (hasCurrencyMetric(parsedLabelTokens)) {
        return formatSensitiveText(resolvedLabel, isPrivacyModeEnabled);
      }
      return resolvedLabel;
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
        gap={0}
        style={{ containerType: "inline-size" }}
      >
        <BodyText tone="primary" size="2rem" fw={700} responsive ta="center">
          {valueText}
        </BodyText>
        {labelText && (
          <BodyText tone="secondary" size="sm" ta="center" responsive>
            {labelText}
          </BodyText>
        )}
      </Stack>
    );
  };

  return (
    <Card
      w="100%"
      h="100%"
      miw={0}
      style={{
        containerType: "inline-size",
        display: "grid",
        gridTemplateRows: "auto minmax(0, 1fr)",
        minHeight: 0,
        overflow: "hidden",
      }}
    >
      <Card.Section p="0.25rem" separatorThickness="thick">
        <Flex
          w="100%"
          justify="center"
          style={{ containerType: "inline-size" }}
        >
          <HeadingText tone="heading" size="1.25rem" responsive>
            {titleText}
          </HeadingText>
        </Flex>
      </Card.Section>
      <Card.Section p="0.5rem">{renderContent()}</Card.Section>
      {settingsOpened !== undefined && onSettingsClose && (
        <MetricWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </Card>
  );
};

export default MetricWidget;
