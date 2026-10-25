import { Stack } from "@mantine/core";
import { Drawer } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  isTransactionTrendsWidgetType,
  WIDGET_REGISTRY,
} from "~/shared/dashboardGrid";
import WidgetPickerItem from "./WidgetPickerItem/WidgetPickerItem";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { useCreateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useCreateWidgetSettingsMutation";

interface WidgetPickerProps {
  opened: boolean;
  onClose: () => void;
}

const WidgetPicker = ({
  opened,
  onClose,
}: WidgetPickerProps): React.ReactNode => {
  const { t } = useTranslation();
  const widgetSettingsQuery = useWidgetSettingsQuery();
  const createWidgetSettingsMutation = useCreateWidgetSettingsMutation();

  const handleAddWidget = (widgetType: string) => {
    const entry = WIDGET_REGISTRY.find((r) => r.widgetType === widgetType);
    if (!entry) {
      showNotification({
        type: NotificationType.Error,
        message: t("error_widget_type_not_found", { widgetType }),
      });
      return;
    }

    const existing = widgetSettingsQuery.data ?? [];
    const nextSmY = existing.reduce(
      (max, w) => Math.max(max, w.smY + w.smH),
      0,
    );

    createWidgetSettingsMutation.mutate({
      widgetType,
      lgX: 0,
      lgY: 9999,
      lgW: null,
      lgH: null,
      smY: nextSmY,
      smH: null,
    });

    onClose();
  };

  const existingWidgetTypes = React.useMemo(() => {
    return (widgetSettingsQuery.data ?? []).map((w) => w.widgetType);
  }, [widgetSettingsQuery.data]);

  return (
    <Drawer
      opened={opened}
      onClose={onClose}
      title={t("add_widget")}
      position="right"
      size="sm"
    >
      <Stack gap="0.5rem">
        {WIDGET_REGISTRY.map((entry) => {
          const instanceCount = existingWidgetTypes.filter(
            (widgetType) =>
              isTransactionTrendsWidgetType(entry.widgetType)
                ? isTransactionTrendsWidgetType(widgetType)
                : widgetType === entry.widgetType,
          ).length;
          const isDisabled =
            entry.maxInstances !== Infinity &&
            instanceCount >= entry.maxInstances;

          return (
            <WidgetPickerItem
              key={entry.widgetType}
              widget={entry}
              isDisabled={isDisabled}
              handleAdd={handleAddWidget}
            />
          );
        })}
      </Stack>
    </Drawer>
  );
};

export default WidgetPicker;
