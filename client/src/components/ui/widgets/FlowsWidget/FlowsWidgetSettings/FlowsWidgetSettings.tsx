import { Group, Stack } from "@mantine/core";
import { Button, Select } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import Modal from "~/components/core/Modal/Modal";
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
        <Select
          data={monthOptions}
          label={t("flows_widget_month_count_label")}
          description={t("flows_widget_settings_message")}
          {...monthCountField.getInputProps()}
        />
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
