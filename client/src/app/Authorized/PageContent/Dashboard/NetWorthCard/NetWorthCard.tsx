import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import NetWorthItem from "./NetWorthItem/NetWorthItem";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IAccountResponse } from "~/models/account";
import { AxiosResponse } from "axios";
import { filterVisibleAssets } from "~/helpers/assets";
import { IAssetResponse } from "~/models/asset";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  INetWorthWidgetLine,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import {
  calculateLineTotal,
  isNetWorthWidgetType,
  parseNetWorthConfiguration,
} from "~/helpers/widgets";

const NetWorthCard = (): React.ReactNode => {
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

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return [];
    },
  });

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const validAccounts = filterVisibleAccounts(accountsQuery.data ?? []);
  const validAssets = filterVisibleAssets(assetsQuery.data ?? []);

  const getNetWorthLines = (): React.ReactNode => {
    if (
      widgetSettingsQuery.isPending ||
      accountsQuery.isPending ||
      assetsQuery.isPending
    ) {
      return <Skeleton height={200} radius="lg" />;
    }

    if (!widgetSettingsQuery.data || widgetSettingsQuery.data.length === 0) {
      return <DimmedText size="sm">No configuration data found.</DimmedText>;
    }

    const netWorthWidgetSettingsList = widgetSettingsQuery.data
      .slice()
      .filter((widget) => isNetWorthWidgetType(widget.widgetType));

    if (netWorthWidgetSettingsList.length === 0) {
      return (
        <DimmedText size="sm">There was an error loading settings.</DimmedText>
      );
    }

    const configuration = parseNetWorthConfiguration(
      netWorthWidgetSettingsList[0]!.configuration
    );

    if (!configuration) {
      return (
        <DimmedText size="sm">
          There was an error loading the configuration.
        </DimmedText>
      );
    }

    const netWorthWidgetLines = configuration.lines ?? [];

    if (!netWorthWidgetLines || netWorthWidgetLines.length === 0) {
      return (
        <DimmedText size="sm">
          No items are configured for this widget.
        </DimmedText>
      );
    }

    const groupedLines = netWorthWidgetLines.reduce<
      Record<number, INetWorthWidgetLine[]>
    >(
      (
        acc: Record<number, INetWorthWidgetLine[]>,
        line: INetWorthWidgetLine
      ) => {
        const group = line.group ?? 0;
        if (!acc[group]) {
          acc[group] = [];
        }
        acc[group].push(line);
        return acc;
      },
      {} as Record<number, INetWorthWidgetLine[]>
    );

    const orderedGroupedLines = Object.keys(groupedLines)
      .map((key) => Number(key))
      .sort((a, b) => a - b);

    if (orderedGroupedLines.length === 0) {
      return (
        <DimmedText size="sm">
          This widget does not contain any groups.
        </DimmedText>
      );
    }

    return (
      <Stack gap="0.5rem">
        {orderedGroupedLines.map((groupId: number) => {
          const groupLines = (groupedLines[groupId] ??
            []) as INetWorthWidgetLine[];
          const sortedGroupLines = groupLines
            .slice()
            .sort(
              (a: INetWorthWidgetLine, b: INetWorthWidgetLine) =>
                a.index - b.index
            );

          return (
            <Card key={`net-worth-group-${groupId}`} p="0.25rem" elevation={2}>
              <Stack gap={0}>
                {sortedGroupLines.map((line: INetWorthWidgetLine) => (
                  <NetWorthItem
                    key={`${line.group}-${line.index}-${line.name}`}
                    title={line.name}
                    totalBalance={calculateLineTotal(
                      line,
                      validAccounts,
                      validAssets,
                      netWorthWidgetLines
                    )}
                    userCurrency={userSettingsQuery.data?.currency ?? "USD"}
                  />
                ))}
              </Stack>
            </Card>
          );
        })}
      </Stack>
    );
  };

  return (
    <Card w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="xl">Net Worth</PrimaryText>
        {getNetWorthLines()}
      </Stack>
    </Card>
  );
};

export default NetWorthCard;
