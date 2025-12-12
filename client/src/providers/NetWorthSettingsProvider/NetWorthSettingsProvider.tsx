import React from "react";
import { useQuery } from "@tanstack/react-query";
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

interface NetWorthLineGroup {
  groupId: number;
  lines: INetWorthWidgetLine[];
}

interface NetWorthSettingsContextType {
  settingsId: string;
  lineGroups: NetWorthLineGroup[];
  lines: INetWorthWidgetLine[];
}

export const NetWorthSettingsContext =
  React.createContext<NetWorthSettingsContextType>({
    settingsId: "",
    lineGroups: [],
    lines: [],
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
        }
      }
    }
  }, [widgetSettingsQuery.data]);

  const lineGroups = React.useMemo(() => {
    if (!netWorthWidgetSettings?.configuration) {
      return [] as NetWorthLineGroup[];
    }

    return groupLines(netWorthWidgetSettings.configuration.lines ?? []);
  }, [netWorthWidgetSettings]);

  const value = React.useMemo(
    () => ({
      settingsId: netWorthWidgetSettings?.id ?? "",
      lineGroups,
      lines: netWorthWidgetSettings?.configuration.lines ?? [],
      loading: widgetSettingsQuery.isPending,
    }),
    [
      lineGroups,
      netWorthWidgetSettings?.configuration.lines ?? [],
      widgetSettingsQuery.isPending,
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
