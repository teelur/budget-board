import { Button, Group, Stack, Popover as MantinePopover } from "@mantine/core";
import { PlusIcon, RotateCcwIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Popover from "~/components/core/Popover/Popover";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useResetSmallScreenLayoutMutation } from "~/hooks/mutations/widgetSettings/useresetSmallScreenLayoutMutation";
import { useResetToDefaultMutation } from "~/hooks/mutations/widgetSettings/useResetToDefaultMutation";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";

interface DashboardEditorProps {
  onDone: () => void;
  onAddWidget: () => void;
  editTarget: "lg" | "sm";
}

const DashboardEditor = ({
  onDone,
  onAddWidget,
  editTarget,
}: DashboardEditorProps): React.ReactNode => {
  const [isResetPopoverOpen, setIsResetPopoverOpen] = React.useState(false);

  const { t } = useTranslation();
  const widgetSettingsQuery = useWidgetSettingsQuery();
  const resetSmallScreenLayoutMutation = useResetSmallScreenLayoutMutation();
  const resetToDefaultMutation = useResetToDefaultMutation();

  const handleConfirmReset = async () => {
    setIsResetPopoverOpen(false);
    await resetToDefaultMutation.mutateAsync();
  };

  return (
    <Group justify="flex-end" gap="0.5rem">
      {editTarget === "sm" && (
        <Button
          size="xs"
          variant="subtle"
          loading={resetSmallScreenLayoutMutation.isPending}
          onClick={() => resetSmallScreenLayoutMutation.mutate()}
        >
          {t("reset_to_desktop_order")}
        </Button>
      )}
      <Button
        size="xs"
        variant="subtle"
        leftSection={<PlusIcon size={16} />}
        onClick={onAddWidget}
      >
        {t("add_widget")}
      </Button>
      <Popover
        opened={isResetPopoverOpen}
        onChange={setIsResetPopoverOpen}
        position="bottom-end"
        withArrow
      >
        <MantinePopover.Target>
          <Button
            size="xs"
            variant="subtle"
            leftSection={<RotateCcwIcon size={16} />}
            onClick={() => setIsResetPopoverOpen((opened) => !opened)}
            loading={resetToDefaultMutation.isPending}
          >
            {t("reset_dashboard")}
          </Button>
        </MantinePopover.Target>
        <MantinePopover.Dropdown maw={350}>
          <Stack gap={10}>
            <PrimaryText size="xs">{t("reset_dashboard_warning")}</PrimaryText>
            <Group gap="xs" justify="flex-end">
              <Button
                size="xs"
                variant="subtle"
                onClick={() => setIsResetPopoverOpen(false)}
              >
                {t("cancel")}
              </Button>
              <Button
                size="xs"
                color="var(--button-color-destructive)"
                loading={resetToDefaultMutation.isPending}
                disabled={widgetSettingsQuery.isPending}
                onClick={handleConfirmReset}
              >
                {t("confirm_reset_to_defaults")}
              </Button>
            </Group>
          </Stack>
        </MantinePopover.Dropdown>
      </Popover>
      <Button size="xs" onClick={onDone}>
        {t("done_editing")}
      </Button>
    </Group>
  );
};

export default DashboardEditor;
