import { Badge, Button, Group, Stack, Text } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { WIDGET_REGISTRY } from "~/shared/dashboardGrid";

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
      <Stack gap="sm">
        {WIDGET_REGISTRY.map((entry) => {
          const instanceCount = existingWidgetTypes.filter(
            (wt) => wt === entry.widgetType,
          ).length;
          const isDisabled =
            entry.maxInstances !== Infinity &&
            instanceCount >= entry.maxInstances;

          return (
            <Group
              key={entry.widgetType}
              justify="space-between"
              p="sm"
              style={{
                border: "1px solid var(--mantine-color-default-border)",
                borderRadius: "var(--mantine-radius-sm)",
                opacity: isDisabled ? 0.5 : 1,
              }}
            >
              <Stack gap={2} style={{ flex: 1 }}>
                <Group gap="xs">
                  <Text size="sm" fw={500}>
                    {t(entry.labelKey)}
                  </Text>
                  {isDisabled && (
                    <Badge size="xs" variant="light">
                      {t("widget_already_added")}
                    </Badge>
                  )}
                </Group>
                <Text size="xs" c="dimmed">
                  {t(entry.descriptionKey)}
                </Text>
              </Stack>
              <Button
                size="xs"
                variant="light"
                leftSection={<PlusIcon size={12} />}
                disabled={isDisabled}
                onClick={() => handleAdd(entry.widgetType)}
              >
                {t("add_widget")}
              </Button>
            </Group>
          );
        })}
      </Stack>
    </Drawer>
  );
};

export default WidgetPicker;
