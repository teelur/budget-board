import { Button, Group } from "@mantine/core";
import DashboardEditor from "./DashboardEditor/DashboardEditor";
import { LayoutIcon } from "lucide-react";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import WidgetPicker from "./WidgetPicker/WidgetPicker";
import {
  IWidgetSettingsCreateRequest,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import { WIDGET_REGISTRY } from "~/shared/dashboardGrid";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { useTranslation } from "react-i18next";

interface DashboardHeaderProps {
  isEditMode: boolean;
  setIsEditMode: React.Dispatch<React.SetStateAction<boolean>>;
}

const DashboardHeader = ({
  isEditMode,
  setIsEditMode,
}: DashboardHeaderProps): React.ReactNode => {
  const [pickerOpened, { open: openPicker, close: closePicker }] =
    useDisclosure(false);
  const [isResetting, setIsResetting] = React.useState(false);

  const { request } = useAuth();
  const queryClient = useQueryClient();
  const { t } = useTranslation();

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

  const doAddWidget = useMutation({
    mutationFn: async (newWidget: IWidgetSettingsCreateRequest) =>
      await request({
        url: "/api/widgetSettings",
        method: "POST",
        data: newWidget,
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

  const handleAddWidget = (widgetType: string) => {
    const entry = WIDGET_REGISTRY.find((r) => r.widgetType === widgetType);
    if (!entry) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("error_widget_type_not_found", { widgetType }),
      });
      return;
    }

    doAddWidget.mutate({
      widgetType,
      x: 0,
      y: 9999,
      w: null,
      h: null,
    });
  };

  const widgets = widgetSettingsQuery.data ?? [];

  const handleResetToDefaults = async () => {
    setIsResetting(true);
    try {
      await Promise.all(
        widgets.map((w) =>
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

  return (
    <>
      <Group justify="flex-end">
        {isEditMode ? (
          <DashboardEditor
            onDone={() => setIsEditMode(false)}
            onAddWidget={openPicker}
            onResetToDefaults={handleResetToDefaults}
            isResetting={isResetting}
          />
        ) : (
          <Button
            variant="subtle"
            leftSection={<LayoutIcon size={16} />}
            onClick={() => setIsEditMode(true)}
          >
            {t("edit_layout")}
          </Button>
        )}
      </Group>
      <WidgetPicker
        opened={pickerOpened}
        onClose={closePicker}
        existingWidgetTypes={widgets.map((w) => w.widgetType)}
        onAddWidget={handleAddWidget}
      />
    </>
  );
};

export default DashboardHeader;
