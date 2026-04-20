import { Badge, Button, Group, Stack } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
          <PrimaryText size="sm">{t(widget.labelKey)}</PrimaryText>
          {isDisabled && (
            <Badge size="xs" variant="light">
              {t("widget_already_added")}
            </Badge>
          )}
        </Group>
        <DimmedText size="xs">{t(widget.descriptionKey)}</DimmedText>
      </Stack>
      <Button
        size="xs"
        variant="light"
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
