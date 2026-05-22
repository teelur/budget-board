import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { WIDGET_REGISTRY } from "~/shared/dashboardGrid";
import WidgetPickerItem from "./WidgetPickerItem/WidgetPickerItem";
import { notifications } from "@mantine/notifications";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  IWidgetSettingsCreateRequest,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";

interface WidgetPickerProps {
  opened: boolean;
  onClose: () => void;
}

const WidgetPicker = ({
  opened,
  onClose,
}: WidgetPickerProps): React.ReactNode => {
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

  const existingWidgetTypes = (widgetSettingsQuery.data ?? []).map(
    (w) => w.widgetType,
  );

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

    const existing = widgetSettingsQuery.data ?? [];
    const nextSmY = existing.reduce(
      (max, w) => Math.max(max, w.smY + w.smH),
      0,
    );

    doAddWidget.mutate({
      widgetType,
      lgX: 0,
      lgY: 9999,
      lgW: null,
      lgH: null,
      smY: nextSmY,
      smH: null,
    });

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
      <Stack gap="0.5rem">
        {WIDGET_REGISTRY.map((entry) => {
          const instanceCount = existingWidgetTypes.filter(
            (wt) => wt === entry.widgetType,
          ).length;
          const isDisabled =
            entry.maxInstances !== Infinity &&
            instanceCount >= entry.maxInstances;

          return (
            <WidgetPickerItem
              key={entry.widgetType}
              widget={entry}
              isDisabled={isDisabled}
              handleAdd={handleAddWidget}
            />
          );
        })}
      </Stack>
    </Drawer>
  );
};

export default WidgetPicker;
