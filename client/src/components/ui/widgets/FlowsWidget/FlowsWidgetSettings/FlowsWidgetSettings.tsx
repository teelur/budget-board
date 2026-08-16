import { Button, Group, Select, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import Accordion from "~/components/core/Accordion/Accordion";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { parseFlowsConfiguration } from "~/helpers/widgets";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

interface FlowsWidgetSettingsProps {
  widget: IWidgetSettingsResponse;
  opened: boolean;
  onClose: () => void;
}

const FlowsWidgetSettings = ({
  widget,
  opened,
  onClose,
}: FlowsWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const updateWidgetSettingsMutation = useUpdateWidgetSettingsMutation();
  const monthCountField = useField({ initialValue: "1" });
  const [initialized, setInitialized] = React.useState(false);

  const monthOptions = Array.from({ length: 12 }, (_, index) => {
    const monthCount = index + 1;
    return {
      value: monthCount.toString(),
      label: t(monthCount === 1 ? "1_month" : `${monthCount}_months`),
    };
  });

  const handleClose = () => {
    setInitialized(false);
    monthCountField.reset();
    onClose();
  };

  React.useEffect(() => {
    if (!opened || initialized) {
      return;
    }

    monthCountField.setValue(
      parseFlowsConfiguration(widget.configuration).monthCount.toString(),
    );
    setInitialized(true);
  }, [opened, initialized, monthCountField, widget.configuration]);

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title={
        <PrimaryHeading component="span" order={4}>
          {t("flows_widget_settings")}
        </PrimaryHeading>
      }
      size="sm"
    >
      <Stack gap="0.75rem">
        <DimmedText size="sm">{t("flows_widget_settings_message")}</DimmedText>
        <Accordion elevation={1}>
          <Accordion.Item
            defaultOpen
            title={
              <PrimaryText size="sm">
                {t("flows_widget_month_count_label")}
              </PrimaryText>
            }
          >
            <Select
              label={t("flows_widget_month_count_label")}
              data={monthOptions}
              {...monthCountField.getInputProps()}
            />
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
                    monthCount: Number(monthCountField.getValue()),
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

export default FlowsWidgetSettings;
