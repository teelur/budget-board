import { Button, Group } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon, RotateCcwIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { translateAxiosError } from "~/helpers/requests";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
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
  const [isConfirmingReset, setIsConfirmingReset] = React.useState(false);
  const [isResetting, setIsResetting] = React.useState(false);

  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();

  const widgetSettingsQuery = useQuery({
    queryKey: ["widgetSettings"],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });
      if (res.status === 200) {
        return res.data as IWidgetSettingsResponse[];
      }
      return [];
    },
  });

  const doResetMobileLayout = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/widgetSettings/resetSmallScreenLayout",
        method: "POST",
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const handleResetToDefaults = async () => {
    setIsResetting(true);
    try {
      await Promise.all(
        (widgetSettingsQuery.data ?? []).map((w) =>
          request({
            url: "/api/widgetSettings",
            method: "DELETE",
            params: { widgetGuid: w.id },
          }),
        ),
      );
      await queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });
    } catch {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("error_loading_settings_message"),
      });
    } finally {
      setIsResetting(false);
    }
  };

  const handleResetClick = () => {
    if (isConfirmingReset) {
      handleResetToDefaults();
      setIsConfirmingReset(false);
    } else {
      setIsConfirmingReset(true);
    }
  };

  const handleCancelReset = () => {
    setIsConfirmingReset(false);
  };

  return (
    <Group justify="flex-end" gap="0.5rem">
      {editTarget === "sm" && (
        <Button
          size="xs"
          variant="subtle"
          loading={doResetMobileLayout.isPending}
          onClick={() => doResetMobileLayout.mutate()}
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
      {isConfirmingReset ? (
        <Group gap="xs">
          <Button
            size="xs"
            variant="outline"
            color="var(--button-color-destructive)"
            loading={isResetting}
            onClick={handleResetClick}
          >
            {t("confirm_reset_to_defaults")}
          </Button>
          <Button size="xs" variant="subtle" onClick={handleCancelReset}>
            {t("cancel")}
          </Button>
        </Group>
      ) : (
        <Button
          size="xs"
          variant="subtle"
          leftSection={<RotateCcwIcon size={16} />}
          onClick={handleResetClick}
          loading={isResetting}
        >
          {t("reset_to_defaults")}
        </Button>
      )}
      <Button size="xs" onClick={onDone}>
        {t("done_editing")}
      </Button>
    </Group>
  );
};

export default DashboardEditor;
