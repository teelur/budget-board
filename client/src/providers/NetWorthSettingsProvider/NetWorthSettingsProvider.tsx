import React from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  INetWorthWidgetConfiguration,
  INetWorthWidgetLine,
  IWidgetSettingsResponse,
  IWidgetSettingsUpdateRequest,
} from "~/models/widgetSettings";
import {
  isNetWorthWidgetType,
  parseNetWorthConfiguration,
} from "~/helpers/widgets";
import { notifications } from "@mantine/notifications";

interface NetWorthLineGroup {
  groupId: number;
  lines: INetWorthWidgetLine[];
}

interface NetWorthSettingsContextType {
  lineGroups: NetWorthLineGroup[];
  lineNames: string[];
  updateConfiguration: (
    updatedConfiguration: INetWorthWidgetConfiguration
  ) => void;
  saveChanges: () => Promise<void>;
  isSavePending: boolean;
  loading: boolean;
  isDirty: boolean;
  setIsDirty: (dirty: boolean) => void;
  refresh: () => Promise<void>;
}

export const NetWorthSettingsContext =
  React.createContext<NetWorthSettingsContextType>({
    lineGroups: [],
    lineNames: [],
    updateConfiguration: async () => {},
    saveChanges: async () => {},
    isSavePending: false,
    loading: true,
    isDirty: false,
    setIsDirty: () => {},
    refresh: async () => {},
  });

interface NetWorthSettingsProviderProps {
  children: React.ReactNode;
}

const groupLines = (lines: INetWorthWidgetLine[]): NetWorthLineGroup[] => {
  if (!lines || lines.length === 0) {
    return [];
  }

  const grouped = lines.reduce<Record<number, INetWorthWidgetLine[]>>(
    (acc, line) => {
      const group = line.group ?? 0;
      if (!acc[group]) {
        acc[group] = [];
      }
      acc[group].push(line);
      return acc;
    },
    {}
  );

  return Object.keys(grouped)
    .map((key) => Number(key))
    .sort((a, b) => a - b)
    .map((groupId) => ({
      groupId,
      lines: (grouped[groupId] ?? []).slice().sort((a, b) => a.index - b.index),
    }));
};

export const NetWorthSettingsProvider = (
  props: NetWorthSettingsProviderProps
): React.ReactNode => {
  const [netWorthWidgetSettings, setNetWorthWidgetSettings] = React.useState<
    IWidgetSettingsUpdateRequest<INetWorthWidgetConfiguration> | undefined
  >(undefined);

  const [isDirty, setIsDirty] = React.useState<boolean>(false);

  const { request } = useAuth();

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

  const queryClient = useQueryClient();
  const doUpdateWidget = useMutation({
    mutationFn: async (
      updatedSettings: IWidgetSettingsUpdateRequest<INetWorthWidgetConfiguration>
    ) =>
      await request({
        url: `/api/widgetSettings`,
        method: "PUT",
        data: updatedSettings,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Widget settings updated successfully.",
      });
    },
  });

  React.useEffect(() => {
    if (widgetSettingsQuery.data) {
      const foundWidget = widgetSettingsQuery.data.find((ws) =>
        isNetWorthWidgetType(ws.widgetType)
      );
      if (foundWidget) {
        const configuration = parseNetWorthConfiguration(
          foundWidget.configuration
        );

        if (configuration) {
          setNetWorthWidgetSettings({
            ...foundWidget,
            configuration,
          });

          setIsDirty(false);
        }
      }
    }
  }, [widgetSettingsQuery.data]);

  const updateConfiguration = (
    updatedConfiguration: INetWorthWidgetConfiguration
  ) => {
    if (!netWorthWidgetSettings) {
      return;
    }

    setNetWorthWidgetSettings({
      ...netWorthWidgetSettings,
      configuration: updatedConfiguration,
    });
    setIsDirty(true);
  };

  const lineGroups = React.useMemo(() => {
    if (!netWorthWidgetSettings?.configuration) {
      return [] as NetWorthLineGroup[];
    }

    return groupLines(netWorthWidgetSettings.configuration.lines ?? []);
  }, [netWorthWidgetSettings]);

  const lineNames = React.useMemo(() => {
    return (
      netWorthWidgetSettings?.configuration.lines.map((line) => line.name) ?? []
    );
  }, [netWorthWidgetSettings]);

  const saveChanges = React.useCallback(async () => {
    if (netWorthWidgetSettings && isDirty) {
      await doUpdateWidget.mutateAsync(netWorthWidgetSettings);
      setIsDirty(false);
    }
  }, [netWorthWidgetSettings, isDirty, doUpdateWidget]);

  const refresh = React.useCallback(async () => {
    await widgetSettingsQuery.refetch();
  }, [widgetSettingsQuery]);

  const value = React.useMemo(
    () => ({
      lineGroups,
      lineNames,
      updateConfiguration,
      saveChanges,
      isSavePending: doUpdateWidget.isPending,
      loading: widgetSettingsQuery.isPending,
      isDirty,
      setIsDirty,
      refresh,
    }),
    [
      lineGroups,
      lineNames,
      updateConfiguration,
      saveChanges,
      doUpdateWidget.isPending,
      widgetSettingsQuery.isPending,
      isDirty,
      setIsDirty,
      refresh,
    ]
  );

  return (
    <NetWorthSettingsContext.Provider value={value}>
      {props.children}
    </NetWorthSettingsContext.Provider>
  );
};

export const useNetWorthSettings = () =>
  React.useContext(NetWorthSettingsContext);
