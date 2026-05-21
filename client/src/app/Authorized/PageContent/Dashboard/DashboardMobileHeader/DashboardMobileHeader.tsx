import { Badge, Button, Group } from "@mantine/core";
import { LayoutIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface DashboardMobileHeaderProps {
  isEditMode: boolean;
  setIsEditMode: React.Dispatch<React.SetStateAction<boolean>>;
}

const DashboardMobileHeader = ({
  isEditMode,
  setIsEditMode,
}: DashboardMobileHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();

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

  return (
    <Group justify="space-between" align="center">
      <Group>
        {isEditMode && <Badge variant="light">{t("mobile")}</Badge>}
      </Group>
      <Group gap={"0.5rem"}>
        {isEditMode ? (
          <>
            <Button
              variant="subtle"
              size="xs"
              loading={doResetMobileLayout.isPending}
              onClick={() => doResetMobileLayout.mutate()}
            >
              {t("reset_to_desktop_order")}
            </Button>
            <Button size="xs" onClick={() => setIsEditMode(false)}>
              {t("done_editing")}
            </Button>
          </>
        ) : (
          <Button
            size="xs"
            variant="subtle"
            leftSection={<LayoutIcon size={16} />}
            onClick={() => setIsEditMode(true)}
          >
            {t("edit_layout")}
          </Button>
        )}
      </Group>
    </Group>
  );
};

export default DashboardMobileHeader;
