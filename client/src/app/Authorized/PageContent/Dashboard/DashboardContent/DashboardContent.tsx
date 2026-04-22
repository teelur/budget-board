import { Skeleton, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import {
  Layout,
  LayoutItem,
  ResponsiveGridLayout,
  useContainerWidth,
} from "react-grid-layout";
import AccountsWidget from "~/components/ui/widgets/AccountsWidget/AccountsWidget";
import NetWorthWidget from "~/components/ui/widgets/NetWorthWidget/NetWorthWidget";
import WidgetShell from "~/components/ui/widgets/shared/WidgetShell/WidgetShell";
import { translateAxiosError } from "~/helpers/requests";
import {
  IWidgetSettingsBatchUpdateRequest,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  deriveSmLayout,
  GRID_BREAKPOINT,
  GRID_COLS,
  GRID_ROW_HEIGHT,
} from "~/shared/dashboardGrid";
import SpendingTrendsWidget from "../../../../../components/ui/widgets/SpendingTrendsWidget/SpendingTrendsWidget";
import UncategorizedTransactionsWidget from "~/components/ui/widgets/UncategorizedTransactionsWidget/UncategorizedTransactionsWidget";

const SKELETON_COUNT = 4;

interface DashboardContentProps {
  isEditMode: boolean;
  onBreakpointChange?: (breakpoint: string) => void;
}

const DashboardContent = ({
  isEditMode,
  onBreakpointChange,
}: DashboardContentProps) => {
  const [settingsOpenId, setSettingsOpenId] = React.useState<string | null>(
    null,
  );

  const { width, containerRef } = useContainerWidth({ initialWidth: 1280 });
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
        return <SpendingTrendsWidget />;
      case "UncategorizedTransactions":
        return <UncategorizedTransactionsWidget />;
      default:
        return null;
    }
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

  return (
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
          onBreakpointChange={onBreakpointChange}
          margin={[12, 12]}
          containerPadding={[0, 0]}
        >
          {widgets.map((widget) => (
            <div key={widget.id} style={{ height: "100%", overflow: "hidden" }}>
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
  );
};

export default DashboardContent;
