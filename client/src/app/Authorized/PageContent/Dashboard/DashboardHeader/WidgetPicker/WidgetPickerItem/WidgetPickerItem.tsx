import { Group, Stack } from "@mantine/core";
import { Badge, BodyText, Button } from "@teelur/budget-board-ui";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { WidgetRegistryEntry } from "~/shared/dashboardGrid";

interface WidgetPickerItemProps {
  widget: WidgetRegistryEntry;
  isDisabled: boolean;
  handleAdd: (widgetType: string) => void;
}

const WidgetPickerItem = ({
  widget,
  isDisabled,
  handleAdd,
}: WidgetPickerItemProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Group
      key={widget.widgetType}
      justify="space-between"
      p="0.5rem"
      style={{
        border: "1px solid var(--base-color-border)",
        borderRadius: "var(--mantine-radius-sm)",
        opacity: isDisabled ? 0.5 : 1,
      }}
    >
      <Stack gap={0} style={{ flex: 1 }}>
        <Group gap="0.25rem">
          <BodyText tone="primary" size="sm" fw={500}>
            {t(widget.labelKey)}
          </BodyText>
          {isDisabled && (
            <Badge variant="light" color="primary" size="xs">
              {t("widget_already_added")}
            </Badge>
          )}
        </Group>
        <BodyText tone="secondary" size="xs">
          {t(widget.descriptionKey)}
        </BodyText>
      </Stack>
      <Button
        variant="filled"
        color="primary"
        size="xs"
        leftSection={<PlusIcon size={12} />}
        disabled={isDisabled}
        onClick={() => handleAdd(widget.widgetType)}
      >
        {t("add_widget")}
      </Button>
    </Group>
  );
};

export default WidgetPickerItem;
