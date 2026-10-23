import { Group, ScrollArea, Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  Code,
  CollapsibleCard,
  Modal,
  TextInput,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import FormulaTextInput from "./FormulaTextInput/FormulaTextInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { METRIC_RANGE_EXAMPLES } from "~/helpers/metricWidget";

const SYNTAX_EXAMPLES = `@transactions.sum(this_month, type=expense)
@transactions.sum(${METRIC_RANGE_EXAMPLES[0]}, type=expense)
@transactions.sum(${METRIC_RANGE_EXAMPLES[1]}, type=expense)
@transactions.sum(${METRIC_RANGE_EXAMPLES[2]}, type=expense)
@transactions.sum(${METRIC_RANGE_EXAMPLES[3]}, type=expense)
@budgets.percent_used(this_month, category=Groceries)
@budgets.spent(this_month, category=Groceries) of @budgets.total(this_month, category=Groceries)
@goals.percent_complete(name=Emergency Fund)
@goals.current_amount(name=Emergency Fund) of @goals.target(name=Emergency Fund)
@accounts.balance(type=Checking)`;

interface MetricWidgetSettingsProps {
  widget: IWidgetSettingsResponse;
  opened: boolean;
  onClose: () => void;
}

const MetricWidgetSettings = ({
  widget,
  opened,
  onClose,
}: MetricWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories } = useTransactionCategories();
  const { dayjs } = useLocale();
  const updateWidgetSettingsMutation = useUpdateWidgetSettingsMutation();
  const accountsQuery = useAccountsQuery({ enabled: opened });
  const budgetsQuery = useBudgetsQuery({
    months: [dayjs().startOf("month").toDate()],
    enabled: opened,
  });
  const goalsQuery = useGoalsQuery({
    includeInterest: false,
    enabled: opened,
  });

  const titleField = useField({ initialValue: "" });
  const valueField = useField({ initialValue: "" });
  const labelField = useField({ initialValue: "" });
  const [initialized, setInitialized] = React.useState(false);

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

  const handleClose = () => {
    setInitialized(false);
    titleField.reset();
    valueField.reset();
    labelField.reset();
    onClose();
  };

  React.useEffect(() => {
    if (!opened || initialized) {
      return;
    }

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
  }, [opened, initialized, widget]);

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title={t("metric_widget_settings")}
      size="lg"
    >
      <Stack gap="0.5rem">
        <BodyText tone="secondary" size="sm">
          {t("metric_widget_settings_message")}
        </BodyText>
        <Stack gap="0.5rem">
          <TextInput
            label={t("metric_widget_title_label")}
            placeholder={t("metric_widget_title_placeholder")}
            {...titleField.getInputProps()}
          />
          <FormulaTextInput
            label={t("metric_widget_value_label")}
            placeholder={t("metric_widget_value_placeholder")}
            value={valueField.getValue()}
            onChange={valueField.setValue}
            transactionCategories={transactionCategories}
            budgetCategories={budgetCategories}
            goalNames={goalNames}
            accountNames={accountNames}
          />
          <FormulaTextInput
            label={t("metric_widget_label_label")}
            placeholder={t("metric_widget_label_placeholder")}
            value={labelField.getValue()}
            onChange={labelField.setValue}
            transactionCategories={transactionCategories}
            budgetCategories={budgetCategories}
            goalNames={goalNames}
            accountNames={accountNames}
          />
        </Stack>
        <CollapsibleCard
          title={
            <BodyText tone="primary" size="sm" fw={500}>
              {t("metric_widget_syntax_reference")}
            </BodyText>
          }
          defaultExpanded={false}
        >
          <ScrollArea.Autosize mah={320} type="auto">
            <Stack gap="0.5rem">
              <Code block>{t("metric_widget_syntax_schema")}</Code>
              <BodyText tone="secondary" size="xs">
                {t("metric_widget_syntax_sources")}:{"  "}
                <Code>transactions</Code>, <Code>budgets</Code>,{" "}
                <Code>goals</Code>, <Code>accounts</Code>
              </BodyText>
              <BodyText tone="secondary" size="xs">
                {t("metric_widget_syntax_periods")}:{"  "}
                <Code>this_month</Code> <Code>last_month</Code>{" "}
                <Code>this_year</Code> <Code>last_3_months</Code>{" "}
                <Code>last_6_months</Code> <Code>last_12_months</Code>{" "}
                <Code>all_time</Code>
              </BodyText>
              <BodyText tone="secondary" size="xs" fw={500} mt="0.25rem">
                {t("metric_widget_syntax_examples")}:
              </BodyText>
              <Code block style={{ whiteSpace: "pre" }}>
                {SYNTAX_EXAMPLES}
              </Code>
            </Stack>
          </ScrollArea.Autosize>
        </CollapsibleCard>
        <Group gap="0.5rem">
          <Button
            variant="filled"
            color="neutral"
            size="compact-sm"
            flex="1 1 0"
            onClick={handleClose}
          >
            {t("cancel")}
          </Button>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            flex="1 1 0"
            onClick={() => {
              updateWidgetSettingsMutation.mutate([
                {
                  id: widget.id,
                  configuration: {
                    title: titleField.getValue(),
                    value: valueField.getValue(),
                    label: labelField.getValue(),
                  },
                },
              ]);
            }}
            loading={updateWidgetSettingsMutation.isPending}
          >
            {t("save")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default MetricWidgetSettings;
