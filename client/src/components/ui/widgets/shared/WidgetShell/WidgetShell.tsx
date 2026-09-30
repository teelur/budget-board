import { Group } from "@mantine/core";
import { ActionIcon } from "@teelur/budget-board-ui";
import { Maximize2Icon, SettingsIcon, XIcon } from "lucide-react";
import React from "react";
import { useDeleteWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useDeleteWidgetSettingsMutation";

interface WidgetShellProps {
  widgetSettingsId: string;
  isEditMode: boolean;
  onSettingsOpen?: () => void;
  children: React.ReactNode;
}

const WidgetShell = ({
  widgetSettingsId,
  isEditMode,
  onSettingsOpen,
  children,
}: WidgetShellProps): React.ReactNode => {
  const deleteWidgetSettingsMutation = useDeleteWidgetSettingsMutation();

  return (
    <div
      style={{
        position: "relative",
        height: "100%",
        overflow: "hidden",
      }}
    >
      {isEditMode && (
        <Group
          gap={4}
          style={{
            position: "absolute",
            top: 8,
            right: 8,
            zIndex: 10,
          }}
        >
          {onSettingsOpen && (
            <ActionIcon
              variant="ghost"
              color="contrast"
              size="compact-sm"
              onMouseDown={(e) => e.stopPropagation()}
              onClick={(e) => {
                e.stopPropagation();
                onSettingsOpen();
              }}
              aria-label="Widget settings"
            >
              <SettingsIcon size={20} />
            </ActionIcon>
          )}
          <ActionIcon
            variant="filled"
            color="error"
            size="compact-sm"
            onMouseDown={(e) => e.stopPropagation()}
            onClick={(e) => {
              e.stopPropagation();
              deleteWidgetSettingsMutation.mutate([widgetSettingsId]);
            }}
            aria-label="Remove widget"
          >
            <XIcon size={20} />
          </ActionIcon>
        </Group>
      )}
      {isEditMode && (
        <Maximize2Icon
          size={14}
          style={{
            position: "absolute",
            bottom: 6,
            right: 6,
            zIndex: 10,
            color: "var(--mantine-color-dimmed)",
            pointerEvents: "none",
            transform: "rotate(90deg)",
          }}
        />
      )}
      {children}
    </div>
  );
};

export default WidgetShell;
