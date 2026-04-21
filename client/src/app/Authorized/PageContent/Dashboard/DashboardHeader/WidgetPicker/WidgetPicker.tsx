import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { WIDGET_REGISTRY } from "~/shared/dashboardGrid";
import WidgetPickerItem from "./WidgetPickerItem/WidgetPickerItem";

interface WidgetPickerProps {
  opened: boolean;
  onClose: () => void;
  existingWidgetTypes: string[];
  onAddWidget: (widgetType: string) => void;
}

const WidgetPicker = ({
  opened,
  onClose,
  existingWidgetTypes,
  onAddWidget,
}: WidgetPickerProps): React.ReactNode => {
  const { t } = useTranslation();

  const handleAdd = (widgetType: string) => {
    onAddWidget(widgetType);
    onClose();
  };

  return (
    <Drawer
      opened={opened}
      onClose={onClose}
      title={<PrimaryText size="lg">{t("add_widget")}</PrimaryText>}
      position="right"
      size="sm"
    >
      <Stack gap="0.5rem">
        {WIDGET_REGISTRY.map((entry) => {
          const instanceCount = existingWidgetTypes.filter(
            (wt) => wt === entry.widgetType,
          ).length;
          const isDisabled =
            entry.maxInstances !== Infinity &&
            instanceCount >= entry.maxInstances;

          return (
            <WidgetPickerItem
              key={entry.widgetType}
              widget={entry}
              isDisabled={isDisabled}
              handleAdd={handleAdd}
            />
          );
        })}
      </Stack>
    </Drawer>
  );
};

export default WidgetPicker;
