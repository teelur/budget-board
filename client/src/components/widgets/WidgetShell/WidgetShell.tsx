import { ActionIcon, Group } from "@mantine/core";
import { Maximize2Icon, SettingsIcon, XIcon } from "lucide-react";
import React from "react";

interface WidgetShellProps {
  isEditMode: boolean;
  onRemove: () => void;
  onSettingsOpen?: () => void;
  children: React.ReactNode;
}

const WidgetShell = ({
  isEditMode,
  onRemove,
  onSettingsOpen,
  children,
}: WidgetShellProps): React.ReactNode => {
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
              variant="filled"
              size="md"
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
            color="var(--button-color-destructive)"
            size="md"
            onClick={(e) => {
              e.stopPropagation();
              onRemove();
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
