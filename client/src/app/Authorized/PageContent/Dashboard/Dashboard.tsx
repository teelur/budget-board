import { Button, Group, Skeleton, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { LayoutIcon } from "lucide-react";
import {
  Layout,
  LayoutItem,
  ResponsiveGridLayout,
  useContainerWidth,
} from "react-grid-layout";
import React from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  IWidgetSettingsBatchUpdateRequest,
  IWidgetSettingsCreateRequest,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import { translateAxiosError } from "~/helpers/requests";
import {
  deriveSmLayout,
  GRID_BREAKPOINT,
  GRID_COLS,
  GRID_ROW_HEIGHT,
  WIDGET_REGISTRY,
} from "~/shared/dashboardGrid";
import AccountsWidget from "~/components/widgets/AccountsWidget/AccountsWidget";
import NetWorthWidget from "~/components/widgets/NetWorthWidget/NetWorthWidget";
import SpendingTrendsCard from "./SpendingTrendsCard/SpendingTrendsCard";
import UncategorizedTransactionsWidget from "~/components/widgets/UncategorizedTransactionsWidget/UncategorizedTransactionsWidget";
import WidgetShell from "~/components/widgets/shared/WidgetShell/WidgetShell";
import DashboardEditor from "./DashboardEditor/DashboardEditor";
import WidgetPicker from "./WidgetPicker/WidgetPicker";
import Footer from "./Footer/Footer";

const SKELETON_COUNT = 4;

const Dashboard = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();

  const [isEditMode, setIsEditMode] = React.useState(false);
  const [pickerOpened, { open: openPicker, close: closePicker }] =
    useDisclosure(false);
  const [settingsOpenId, setSettingsOpenId] = React.useState<string | null>(
    null,
  );

  const { width, containerRef } = useContainerWidth({ initialWidth: 1280 });

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

  const doRemoveWidget = useMutation({
    mutationFn: async (widgetId: string) =>
      await request({
        url: "/api/widgetSettings",
        method: "DELETE",
        params: { widgetGuid: widgetId },
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

  const doBatchUpdate = useMutation({
    mutationFn: async (updates: IWidgetSettingsBatchUpdateRequest[]) =>
      await request({
        url: "/api/widgetSettings/batch",
        method: "PUT",
        data: updates,
      }),
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const [isResetting, setIsResetting] = React.useState(false);
  const handleResetToDefaults = async () => {
    setIsResetting(true);
    try {
      const currentWidgets = widgetSettingsQuery.data ?? [];
      await Promise.all(
        currentWidgets.map((w) =>
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

  const handleAddWidget = (widgetType: string) => {
    const entry = WIDGET_REGISTRY.find((r) => r.widgetType === widgetType);
    if (!entry) {
      return;
    }

    doAddWidget.mutate({
      widgetType,
      x: 0,
      y: 9999,
      w: entry.defaultW,
      h: entry.defaultH,
    });
  };

  const handleLayoutChange = (
    layout: Layout,
    _layouts: Partial<Record<string, Layout>>,
  ) => {
    if (!isEditMode) {
      return;
    }

    const updates: IWidgetSettingsBatchUpdateRequest[] = layout.map((item) => ({
      id: item.i,
      x: item.x,
      y: item.y,
      w: item.w,
      h: item.h,
    }));

    doBatchUpdate.mutate(updates);
  };

  const widgets = widgetSettingsQuery.data ?? [];

  const lgLayout: LayoutItem[] = widgets.map((w) => ({
    i: w.id,
    x: w.x,
    y: w.y,
    w: w.w,
    h: w.h,
  }));

  const smLayout = deriveSmLayout(lgLayout);

  const renderWidgetContent = (widget: IWidgetSettingsResponse) => {
    switch (widget.widgetType) {
      case "Accounts":
        return (
          <AccountsWidget
            widgetId={widget.id}
            settingsOpened={settingsOpenId === widget.id}
            onSettingsClose={() => setSettingsOpenId(null)}
          />
        );
      case "NetWorth":
        return (
          <NetWorthWidget
            widgetId={widget.id}
            settingsOpened={settingsOpenId === widget.id}
            onSettingsClose={() => setSettingsOpenId(null)}
          />
        );
      case "SpendingTrends":
        return <SpendingTrendsCard />;
      case "UncategorizedTransactions":
        return <UncategorizedTransactionsWidget />;
      default:
        return null;
    }
  };

  return (
    <Stack w="100%" flex="1" justify="space-between">
      <Stack gap={0}>
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
        <div ref={containerRef} style={{ width: "100%" }}>
          {widgetSettingsQuery.isPending ? (
            <Stack gap="md">
              {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
                <Skeleton key={i} height={GRID_ROW_HEIGHT * 5} radius="md" />
              ))}
            </Stack>
          ) : (
            <ResponsiveGridLayout
              width={width}
              layouts={{ lg: lgLayout, sm: smLayout }}
              breakpoints={{ lg: GRID_BREAKPOINT, sm: 0 }}
              cols={{ lg: GRID_COLS, sm: 1 }}
              rowHeight={GRID_ROW_HEIGHT}
              dragConfig={{ enabled: isEditMode }}
              resizeConfig={{ enabled: isEditMode }}
              onLayoutChange={handleLayoutChange}
              margin={[12, 12]}
            >
              {widgets.map((widget) => (
                <div
                  key={widget.id}
                  style={{ height: "100%", overflow: "hidden" }}
                >
                  <WidgetShell
                    isEditMode={isEditMode}
                    onRemove={() => doRemoveWidget.mutate(widget.id)}
                    onSettingsOpen={
                      widget.widgetType === "NetWorth" ||
                      widget.widgetType === "Accounts"
                        ? () => setSettingsOpenId(widget.id)
                        : undefined
                    }
                  >
                    {renderWidgetContent(widget)}
                  </WidgetShell>
                </div>
              ))}
            </ResponsiveGridLayout>
          )}
        </div>
      </Stack>
      <Footer />
      <WidgetPicker
        opened={pickerOpened}
        onClose={closePicker}
        existingWidgetTypes={widgets.map((w) => w.widgetType)}
        onAddWidget={handleAddWidget}
      />
    </Stack>
  );
};

export default Dashboard;
