import { Button, Group, Stack, Popover as MantinePopover } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useQueryClient } from "@tanstack/react-query";
import { PlusIcon, RotateCcwIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Popover from "~/components/core/Popover/Popover";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { widgetSettingsQueryKey } from "~/helpers/requests";
import { useResetSmallScreenLayoutMutation } from "~/hooks/mutations/widgetSettings/useresetSmallScreenLayoutMutation";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

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
  const [isResetting, setIsResetting] = React.useState(false);

  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();
  const widgetSettingsQuery = useWidgetSettingsQuery();
  const resetSmallScreenLayoutMutation = useResetSmallScreenLayoutMutation();

  // TODO: Fix this
  const handleResetToDefaults = async () => {
    setIsResetting(true);
    try {
      await Promise.all(
        (widgetSettingsQuery.data ?? []).map((w) =>
          request({
            url: "/api/widgetSettings",
            method: "DELETE",
            params: { widgetSettingsId: w.id },
          }),
        ),
      );
      await queryClient.invalidateQueries({
        queryKey: [widgetSettingsQueryKey],
      });
    } catch {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("error_loading_settings_message"),
      });
    } finally {
      setIsResetting(false);
    }
  };

  const handleConfirmReset = async () => {
    setIsResetPopoverOpen(false);
    await handleResetToDefaults();
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
            loading={isResetting}
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
                loading={isResetting}
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
