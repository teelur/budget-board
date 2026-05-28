import { Flex, Skeleton, Text } from "@mantine/core";
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
  parseMetricMarkup,
  resolveTemplate,
  MetricDataContext,
} from "~/helpers/metricWidget";
import { IBudget } from "~/models/budget";
import { IGoalResponse } from "~/models/goal";
import { IAccountResponse } from "~/models/account";
import { IAccountType } from "~/models/accountType";
import { ITransaction } from "~/models/transaction";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import MetricWidgetSettings from "./MetricWidgetSettings/MetricWidgetSettings";
import classes from "./MetricWidget.module.css";

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
  const { intlLocale } = useLocale();

  // ── Widget settings (to read the markup) ──────────────────────────────────

  const widgetSettingsQuery = useQuery({
    queryKey: ["widgetSettings"],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });
      if (res.status === 200) return res.data as IWidgetSettingsResponse[];
      return [];
    },
  });

  const markup = React.useMemo(() => {
    const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
    if (!widget?.configuration) return "";
    try {
      const parsed = JSON.parse(widget.configuration) as { markup?: string };
      return parsed.markup ?? "";
    } catch {
      return "";
    }
  }, [widgetSettingsQuery.data, widgetId]);

  const parsedMarkup = React.useMemo(() => parseMetricMarkup(markup), [markup]);
  const requirements = React.useMemo(
    () => buildDataRequirements(parsedMarkup),
    [parsedMarkup],
  );

  // ── Conditional data fetching ──────────────────────────────────────────────

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
    queryKey: ["transactions", { allTime: true }],
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
      ? requirements.budgetMonths.map((date) => ({
          queryKey: ["budgets", date],
          queryFn: async (): Promise<IBudget[]> => {
            const res: AxiosResponse = await request({
              url: "/api/budget",
              method: "GET",
              params: { date },
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
    queryKey: ["goals", { includeInterest: false }],
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

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });
      if (res.status === 200) return res.data as IAccountResponse[];
      return [];
    },
    enabled: requirements.needsAccounts,
  });

  const accountTypesQuery = useQuery({
    queryKey: ["accountTypes"],
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

  // ── Loading state ──────────────────────────────────────────────────────────

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

  // ── Build data context for expression evaluation ──────────────────────────

  const ctx: MetricDataContext = React.useMemo(
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

  // ── Resolved display values ────────────────────────────────────────────────

  const titleText = React.useMemo(() => {
    if (parsedMarkup.title && !isPending) {
      return resolveTemplate(parsedMarkup.title, ctx);
    }
    return t("metric");
  }, [parsedMarkup.title, isPending, ctx, t]);

  const valueText = React.useMemo(() => {
    if (parsedMarkup.value && !isPending) {
      return resolveTemplate(parsedMarkup.value, ctx);
    }
    return null;
  }, [parsedMarkup.value, isPending, ctx]);

  const labelText = React.useMemo(() => {
    if (parsedMarkup.label && !isPending) {
      return resolveTemplate(parsedMarkup.label, ctx);
    }
    return null;
  }, [parsedMarkup.label, isPending, ctx]);

  // ── Render ─────────────────────────────────────────────────────────────────

  const renderContent = () => {
    if (isPending) {
      return (
        <Flex h="100%" w="100%" p="0.5rem">
          <Skeleton flex={1} radius="md" />
        </Flex>
      );
    }

    if (!parsedMarkup.value) {
      return (
        <WidgetErrorMessage messageKey="metric_widget_no_value_expression" />
      );
    }

    return (
      <Flex
        h="100%"
        w="100%"
        direction="column"
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
      </Flex>
    );
  };

  return (
    <SplitCard
      w="100%"
      h="100%"
      border={BorderThickness.Thick}
      header={
        <PrimaryHeading order={3} lh={1}>
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
