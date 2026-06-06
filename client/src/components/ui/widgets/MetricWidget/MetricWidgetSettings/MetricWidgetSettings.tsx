import {
  Button,
  Code,
  Group,
  ScrollArea,
  Skeleton,
  Stack,
  TextInput,
} from "@mantine/core";
import Accordion from "~/components/core/Accordion/Accordion";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { translateAxiosError } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";
import { IBudget } from "~/models/budget";
import { IGoalResponse } from "~/models/goal";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import FormulaTextInput from "./FormulaTextInput/FormulaTextInput";

const SYNTAX_EXAMPLES = `@transactions.sum(this_month, type=expense)
@budgets.percent_used(this_month, category=Groceries)
@budgets.spent(this_month, category=Groceries) of @budgets.total(this_month, category=Groceries)
@goals.percent_complete(name=Emergency Fund)
@goals.current_amount(name=Emergency Fund) of @goals.target(name=Emergency Fund)
@accounts.balance(type=Checking)`;

interface MetricWidgetSettingsProps {
  widgetId: string;
  opened: boolean;
  onClose: () => void;
}

const MetricWidgetSettings = ({
  widgetId,
  opened,
  onClose,
}: MetricWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const { allTransactionCategories } = useTransactionCategories();
  const queryClient = useQueryClient();

  const titleField = useField({ initialValue: "" });
  const valueField = useField({ initialValue: "" });
  const labelField = useField({ initialValue: "" });
  const [initialized, setInitialized] = React.useState(false);

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

  const goalsQuery = useQuery({
    queryKey: ["goals", { includeInterest: false }],
    queryFn: async (): Promise<IGoalResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/goal",
        method: "GET",
        params: { includeInterest: false },
      });

      if (res.status === 200) {
        return res.data as IGoalResponse[];
      }

      return [];
    },
    enabled: opened,
  });

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
    enabled: opened,
  });

  const currentMonth = React.useMemo(() => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), 1);
  }, []);

  const budgetsQuery = useQuery({
    queryKey: ["budgets", currentMonth],
    queryFn: async (): Promise<IBudget[]> => {
      const res: AxiosResponse = await request({
        url: "/api/budget",
        method: "GET",
        params: { date: currentMonth },
      });

      if (res.status === 200) {
        return res.data as IBudget[];
      }

      return [];
    },
    enabled: opened,
  });

  const transactionCategories = React.useMemo(
    () =>
      Array.from(
        new Set(
          allTransactionCategories
            .map((category) => category.value)
            .filter(Boolean),
        ),
      ).sort((a, b) => a.localeCompare(b)),
    [allTransactionCategories],
  );

  const budgetCategories = React.useMemo(
    () =>
      Array.from(
        new Set((budgetsQuery.data ?? []).map((budget) => budget.category)),
      )
        .filter(Boolean)
        .sort((a, b) => a.localeCompare(b)),
    [budgetsQuery.data],
  );

  const goalNames = React.useMemo(
    () =>
      Array.from(new Set((goalsQuery.data ?? []).map((goal) => goal.name)))
        .filter(Boolean)
        .sort((a, b) => a.localeCompare(b)),
    [goalsQuery.data],
  );

  const accountNames = React.useMemo(
    () =>
      Array.from(
        new Set((accountsQuery.data ?? []).map((account) => account.name)),
      )
        .filter(Boolean)
        .sort((a, b) => a.localeCompare(b)),
    [accountsQuery.data],
  );

  React.useEffect(() => {
    if (!opened) {
      setInitialized(false);
      titleField.reset();
      valueField.reset();
      labelField.reset();
      return;
    }
    if (initialized || widgetSettingsQuery.isPending) return;

    const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
    if (widget?.configuration) {
      try {
        const parsed = JSON.parse(widget.configuration) as {
          title?: string;
          value?: string;
          label?: string;
        };
        titleField.setValue(parsed.title ?? "");
        valueField.setValue(parsed.value ?? "");
        labelField.setValue(parsed.label ?? "");
      } catch {
        titleField.setValue("");
        valueField.setValue("");
        labelField.setValue("");
      }
    }
    setInitialized(true);
  }, [
    opened,
    initialized,
    widgetSettingsQuery.isPending,
    widgetSettingsQuery.data,
    widgetId,
  ]);

  const doSave = useMutation({
    mutationFn: async ({
      title,
      value,
      label,
    }: {
      title: string;
      value: string;
      label: string;
    }) => {
      const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
      if (!widget) {
        throw new Error(t("widget_not_found"));
      }

      return await request({
        url: "/api/widgetSettings",
        method: "PUT",
        data: {
          id: widget.id,
          lgX: widget.lgX,
          lgY: widget.lgY,
          lgW: widget.lgW,
          lgH: widget.lgH,
          smY: widget.smY,
          smH: widget.smH,
          configuration: { title, value, label },
        },
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });
    },
    onError: (error: AxiosError | Error) => {
      const message =
        error instanceof AxiosError
          ? translateAxiosError(error)
          : error.message;
      notifications.show({
        color: "var(--button-color-destructive)",
        message,
      });
    },
  });

  const handleSave = () => {
    doSave.mutate({
      title: titleField.getValue(),
      value: valueField.getValue(),
      label: labelField.getValue(),
    });
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={
        <PrimaryHeading component="span" order={4}>
          {t("metric_widget_settings")}
        </PrimaryHeading>
      }
      size="lg"
    >
      <Stack gap="0.75rem">
        <DimmedText size="sm">{t("metric_widget_settings_message")}</DimmedText>

        {widgetSettingsQuery.isPending ? (
          <Skeleton height={200} radius="md" />
        ) : (
          <Stack gap="0.75rem">
            <TextInput
              label={
                <PrimaryText size="sm">
                  {t("metric_widget_title_label")}
                </PrimaryText>
              }
              placeholder={t("metric_widget_title_placeholder")}
              {...titleField.getInputProps()}
            />
            <FormulaTextInput
              label={
                <PrimaryText size="sm">
                  {t("metric_widget_value_label")}
                </PrimaryText>
              }
              placeholder={t("metric_widget_value_placeholder")}
              value={valueField.getValue()}
              onChange={valueField.setValue}
              transactionCategories={transactionCategories}
              budgetCategories={budgetCategories}
              goalNames={goalNames}
              accountNames={accountNames}
            />
            <FormulaTextInput
              label={
                <PrimaryText size="sm">
                  {t("metric_widget_label_label")}
                </PrimaryText>
              }
              placeholder={t("metric_widget_label_placeholder")}
              value={labelField.getValue()}
              onChange={labelField.setValue}
              transactionCategories={transactionCategories}
              budgetCategories={budgetCategories}
              goalNames={goalNames}
              accountNames={accountNames}
            />
          </Stack>
        )}

        <Accordion elevation={0}>
          <Accordion.Item
            defaultOpen={false}
            title={
              <DimmedText size="sm">
                {t("metric_widget_syntax_reference")}
              </DimmedText>
            }
          >
            <ScrollArea.Autosize mah={320} type="auto">
              <Stack gap="0.5rem">
                <Code block style={{ fontSize: "0.75rem" }}>
                  {t("metric_widget_syntax_schema")}
                </Code>
                <DimmedText size="xs">
                  {t("metric_widget_syntax_sources")}:{"  "}
                  <Code>transactions</Code>, <Code>budgets</Code>,{" "}
                  <Code>goals</Code>, <Code>accounts</Code>
                </DimmedText>
                <DimmedText size="xs">
                  {t("metric_widget_syntax_periods")}:{"  "}
                  <Code>this_month</Code> <Code>last_month</Code>{" "}
                  <Code>this_year</Code> <Code>last_3_months</Code>{" "}
                  <Code>last_6_months</Code> <Code>last_12_months</Code>{" "}
                  <Code>all_time</Code>
                </DimmedText>
                <DimmedText size="xs">
                  {t("metric_widget_syntax_formats")}:{"  "}
                  <Code>currency</Code> <Code>percent</Code>{" "}
                  <Code>integer</Code> <Code>decimal</Code> <Code>number</Code>
                </DimmedText>
                <DimmedText size="xs" fw={600} mt="0.25rem">
                  {t("metric_widget_syntax_examples")}:
                </DimmedText>
                <Code block style={{ fontSize: "0.75rem", whiteSpace: "pre" }}>
                  {SYNTAX_EXAMPLES}
                </Code>
              </Stack>
            </ScrollArea.Autosize>
          </Accordion.Item>
        </Accordion>

        <Group w="100%" justify="flex-end" mt="xs" gap="0.5rem">
          <Button flex={1} variant="default" onClick={onClose}>
            {t("cancel")}
          </Button>
          <Button
            flex={1}
            onClick={handleSave}
            loading={doSave.isPending}
            disabled={widgetSettingsQuery.isPending}
          >
            {t("save")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default MetricWidgetSettings;
