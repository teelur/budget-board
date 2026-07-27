import {
  Button,
  Code,
  Group,
  ScrollArea,
  Stack,
  TextInput,
} from "@mantine/core";
import Accordion from "~/components/core/Accordion/Accordion";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import FormulaTextInput from "./FormulaTextInput/FormulaTextInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

const SYNTAX_EXAMPLES = `@transactions.sum(this_month, type=expense)
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
      title={
        <PrimaryHeading component="span" order={4}>
          {t("metric_widget_settings")}
        </PrimaryHeading>
      }
      size="lg"
    >
      <Stack gap="0.75rem">
        <DimmedText size="sm">{t("metric_widget_settings_message")}</DimmedText>
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
        <Accordion elevation={1}>
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
          <Button flex={1} variant="default" onClick={handleClose}>
            {t("cancel")}
          </Button>
          <Button
            flex={1}
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
