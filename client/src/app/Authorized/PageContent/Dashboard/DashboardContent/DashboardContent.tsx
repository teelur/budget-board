import { Box, Flex, Skeleton, Stack } from "@mantine/core";
import { useMediaQuery } from "@mantine/hooks";
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
  GRID_BREAKPOINT,
  GRID_COLS,
  GRID_ROW_HEIGHT,
} from "~/shared/dashboardGrid";
import SpendingTrendsWidget from "../../../../../components/ui/widgets/SpendingTrendsWidget/SpendingTrendsWidget";
import UncategorizedTransactionsWidget from "~/components/ui/widgets/UncategorizedTransactionsWidget/UncategorizedTransactionsWidget";
import MetricWidget from "~/components/ui/widgets/MetricWidget/MetricWidget";

const SKELETON_COUNT = 4;
const SM_PREVIEW_WIDTH = 500;

interface DashboardContentProps {
  isEditMode: boolean;
  editTarget: "lg" | "sm";
}

const DashboardContent = ({
  isEditMode,
  editTarget,
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
  const widgets = widgetSettingsQuery.data ?? [];
  const lgLayout = React.useMemo<LayoutItem[]>(
    () =>
      widgets.map((w) => ({
        i: w.id,
        x: w.lgX,
        y: w.lgY,
        w: w.lgW,
        h: w.lgH,
      })),
    [widgets],
  );
  const smLayout = React.useMemo<LayoutItem[]>(
    () =>
      widgets.map((w) => ({
        i: w.id,
        x: 0, // x is ignored for sm since cols=1
        y: w.smY,
        w: 1, // w is ignored for sm since cols=1
        h: w.smH,
      })),
    [widgets],
  );

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

  const handleSave = React.useCallback(
    (layout: Layout) => {
      if (!isEditMode) return;

      const currentLayout = editTarget === "lg" ? lgLayout : smLayout;
      const hasChanged = layout.some((item) => {
        const current = currentLayout.find((c) => c.i === item.i);
        if (!current) return true;
        return editTarget === "lg"
          ? current.x !== item.x ||
              current.y !== item.y ||
              current.w !== item.w ||
              current.h !== item.h
          : current.y !== item.y || current.h !== item.h;
      });

      if (!hasChanged) return;

      const updates: IWidgetSettingsBatchUpdateRequest[] =
        editTarget === "lg"
          ? layout.map((item) => ({
              id: item.i,
              lgX: item.x,
              lgY: item.y,
              lgW: item.w,
              lgH: item.h,
            }))
          : layout.map((item) => ({
              id: item.i,
              smY: item.y,
              smH: item.h,
            }));

      doBatchUpdate.mutate(updates);
    },
    [isEditMode, editTarget, lgLayout, smLayout, doBatchUpdate.mutate],
  );

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
      case "Metric":
        return (
          <MetricWidget
            widgetId={widget.id}
            settingsOpened={settingsOpenId === widget.id}
            onSettingsClose={() => setSettingsOpenId(null)}
          />
        );
      default:
        return null;
    }
  };

  const isDesktopViewport =
    useMediaQuery(`(min-width: ${GRID_BREAKPOINT}px)`) ?? false;
  const isEditingSmOnDesktop =
    isEditMode && editTarget === "sm" && isDesktopViewport;

  return (
    <Flex ref={containerRef} w={"100%"} flex="1" justify="center">
      {widgetSettingsQuery.isPending ? (
        <Stack gap="md">
          {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
            <Skeleton key={i} height={GRID_ROW_HEIGHT * 5} radius="md" />
          ))}
        </Stack>
      ) : (
        <Box w={isEditingSmOnDesktop ? SM_PREVIEW_WIDTH : "100%"}>
          <ResponsiveGridLayout
            key={isEditingSmOnDesktop ? "sm-preview" : "lg"}
            width={isEditingSmOnDesktop ? SM_PREVIEW_WIDTH : width}
            layouts={
              isEditingSmOnDesktop
                ? { lg: smLayout, sm: smLayout }
                : { lg: lgLayout, sm: smLayout }
            }
            breakpoints={{ lg: GRID_BREAKPOINT, sm: 0 }}
            cols={
              isEditingSmOnDesktop ? { lg: 1, sm: 1 } : { lg: GRID_COLS, sm: 1 }
            }
            rowHeight={GRID_ROW_HEIGHT}
            dragConfig={{ enabled: isEditMode && settingsOpenId === null }}
            resizeConfig={{ enabled: isEditMode && settingsOpenId === null }}
            onDragStop={(layout) => handleSave(layout)}
            onResizeStop={(layout) => handleSave(layout)}
            margin={[12, 12]}
            containerPadding={[0, 0]}
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
                    widget.widgetType === "Accounts" ||
                    widget.widgetType === "Metric"
                      ? () => setSettingsOpenId(widget.id)
                      : undefined
                  }
                >
                  {renderWidgetContent(widget)}
                </WidgetShell>
              </div>
            ))}
          </ResponsiveGridLayout>
        </Box>
      )}
    </Flex>
  );
};

export default DashboardContent;
