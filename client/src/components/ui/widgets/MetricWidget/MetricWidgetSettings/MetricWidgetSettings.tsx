import {
  Accordion,
  Button,
  Code,
  Group,
  ScrollArea,
  Skeleton,
  Stack,
  TextInput,
} from "@mantine/core";
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
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

const SYNTAX_EXAMPLES = `@transactions.sum(this_month, type=expense){currency}
@budgets.percent_used(this_month, category=Groceries){percent}
@budgets.spent(this_month, category=Groceries){currency} of @budgets.total(this_month, category=Groceries){currency}
@goals.percent_complete(name=Emergency Fund){percent}
@goals.current_amount(name=Emergency Fund){currency} of @goals.target(name=Emergency Fund){currency}
@accounts.balance(type=Checking){currency}`;

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
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("widget_not_found"),
        });
        return;
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
      onClose();
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
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
        <PrimaryHeading order={4}>{t("metric_widget_settings")}</PrimaryHeading>
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
            <TextInput
              label={
                <PrimaryText size="sm">
                  {t("metric_widget_value_label")}
                </PrimaryText>
              }
              placeholder={t("metric_widget_value_placeholder")}
              styles={{
                input: { fontFamily: "monospace", fontSize: "0.85rem" },
              }}
              {...valueField.getInputProps()}
            />
            <TextInput
              label={
                <PrimaryText size="sm">
                  {t("metric_widget_label_label")}
                </PrimaryText>
              }
              placeholder={t("metric_widget_label_placeholder")}
              styles={{
                input: { fontFamily: "monospace", fontSize: "0.85rem" },
              }}
              {...labelField.getInputProps()}
            />
          </Stack>
        )}

        <Accordion variant="contained">
          <Accordion.Item value="reference">
            <Accordion.Control>
              <DimmedText size="sm">
                {t("metric_widget_syntax_reference")}
              </DimmedText>
            </Accordion.Control>
            <Accordion.Panel>
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
                    <Code>integer</Code> <Code>decimal</Code>{" "}
                    <Code>number</Code>
                  </DimmedText>
                  <DimmedText size="xs" fw={600} mt="0.25rem">
                    {t("metric_widget_syntax_examples")}:
                  </DimmedText>
                  <Code
                    block
                    style={{ fontSize: "0.75rem", whiteSpace: "pre" }}
                  >
                    {SYNTAX_EXAMPLES}
                  </Code>
                </Stack>
              </ScrollArea.Autosize>
            </Accordion.Panel>
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
