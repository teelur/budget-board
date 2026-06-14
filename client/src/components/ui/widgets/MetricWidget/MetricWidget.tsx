import { Flex, Skeleton, Stack, Text } from "@mantine/core";
import { useQuery, useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
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
  parseTemplate,
  resolveTemplate,
  MetricDataContext,
} from "~/helpers/metricWidget";
import { IBudget } from "~/models/budget";
import { IGoalResponse } from "~/models/goal";
import { IAccountType } from "~/models/accountType";
import { ITransaction } from "~/models/transaction";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import MetricWidgetSettings from "./MetricWidgetSettings/MetricWidgetSettings";
import classes from "./MetricWidget.module.css";
import {
  accountTypesQueryKey,
  budgetsQueryKey,
  goalsQueryKey,
  transactionsQueryKey,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";

interface MetricWidgetProps {
  widgetId: string;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const MetricWidget = ({
  widgetId,
  settingsOpened,
  onSettingsClose,
}: MetricWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const { preferredCurrency } = useUserSettings();
  const { intlLocale, dayjs } = useLocale();

  const widgetSettingsQuery = useQuery({
    queryKey: [widgetSettingsQueryKey],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });
      if (res.status === 200) return res.data as IWidgetSettingsResponse[];
      return [];
    },
  });

  // The widget configuration is stored as a JSON string in the database.
  const { configTitle, configValue, configLabel } = React.useMemo(() => {
    const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
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
  }, [widgetSettingsQuery.data, widgetId]);

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

  const transactionMonthQueries = useQueries({
    queries:
      requirements.needsTransactions && !requirements.needsAllTimeTransactions
        ? requirements.transactionMonths.map((date) => ({
            queryKey: [
              "transactions",
              { month: date.getMonth(), year: date.getFullYear() },
            ],
            queryFn: async (): Promise<ITransaction[]> => {
              const res: AxiosResponse = await request({
                url: "/api/transaction",
                method: "GET",
                params: {
                  month: date.getMonth() + 1,
                  year: date.getFullYear(),
                },
              });
              if (res.status === 200) return res.data as ITransaction[];
              return [];
            },
          }))
        : [],
    combine: (results) => ({
      data: results.flatMap((r) => r.data ?? []),
      isPending: results.length > 0 && results.some((r) => r.isPending),
    }),
  });

  const allTimeTransactionsQuery = useQuery({
    queryKey: [transactionsQueryKey, { allTime: true }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
      });
      if (res.status === 200) return res.data as ITransaction[];
      return [];
    },
    enabled:
      requirements.needsTransactions && requirements.needsAllTimeTransactions,
  });

  const budgetQueries = useQueries({
    queries: requirements.needsBudgets
      ? requirements.budgetMonths.map((date: Date) => ({
          queryKey: [budgetsQueryKey, dayjs(date).format("YYYY-MM")],
          queryFn: async (): Promise<IBudget[]> => {
            const res: AxiosResponse = await request({
              url: "/api/budget",
              method: "GET",
              params: { month: dayjs(date).format("YYYY-MM-DD") },
            });
            if (res.status === 200) return res.data as IBudget[];
            return [];
          },
        }))
      : [],
    combine: (results) => ({
      data: results.flatMap((r) => r.data ?? []),
      isPending: results.length > 0 && results.some((r) => r.isPending),
    }),
  });

  const goalsQuery = useQuery({
    queryKey: [goalsQueryKey, { includeInterest: false }],
    queryFn: async (): Promise<IGoalResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/goal",
        method: "GET",
        params: { includeInterest: false },
      });
      if (res.status === 200) return res.data as IGoalResponse[];
      return [];
    },
    enabled: requirements.needsGoals,
  });

  const accountsQuery = useAccountsQuery({
    enabled: requirements.needsAccounts,
  });

  const accountTypesQuery = useQuery({
    queryKey: [accountTypesQueryKey],
    queryFn: async (): Promise<IAccountType[]> => {
      const res: AxiosResponse = await request({
        url: "/api/accountType",
        method: "GET",
      });
      if (res.status === 200) return res.data as IAccountType[];
      return [];
    },
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
      (accountsQuery.isPending || accountTypesQuery.isPending));

  const metricDataContext: MetricDataContext = React.useMemo(
    () => ({
      transactions: requirements.needsAllTimeTransactions
        ? (allTimeTransactionsQuery.data ?? [])
        : transactionMonthQueries.data,
      budgets: budgetQueries.data,
      goals: goalsQuery.data ?? [],
      accounts: accountsQuery.data ?? [],
      accountTypes: accountTypesQuery.data ?? [],
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
      accountTypesQuery.data,
      preferredCurrency,
      intlLocale,
    ],
  );

  const titleText = React.useMemo(() => {
    return configTitle || t("metric");
  }, [configTitle, t]);

  const valueText = React.useMemo(() => {
    if (configValue && !isPending) {
      return resolveTemplate(parsedValueTokens, metricDataContext);
    }
    return null;
  }, [configValue, isPending, parsedValueTokens, metricDataContext]);

  const labelText = React.useMemo(() => {
    if (configLabel && !isPending) {
      return resolveTemplate(parsedLabelTokens, metricDataContext);
    }
    return null;
  }, [configLabel, isPending, parsedLabelTokens, metricDataContext]);

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
          widgetId={widgetId}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default MetricWidget;
