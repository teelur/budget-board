import { Group, ScrollArea, Skeleton, Stack } from "@mantine/core";
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
  parseNetWorthConfiguration,
} from "~/helpers/widgets";
import NetWorthCardSettings from "./NetWorthCardSettings/NetWorthCardSettings";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { TrendingUpIcon } from "lucide-react";

interface NetWorthCardProps {
  widgetId: string;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const NetWorthCard = ({
  widgetId,
  settingsOpened,
  onSettingsClose,
}: NetWorthCardProps): React.ReactNode => {
  const { t } = useTranslation();
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
      return (
        <DimmedText size="sm">{t("no_configuration_data_found")}</DimmedText>
      );
    }

    const netWorthWidgetSettingsList = widgetSettingsQuery.data
      .slice()
      .filter((widget) => widget.id === widgetId);

    if (netWorthWidgetSettingsList.length === 0) {
      return (
        <DimmedText size="sm">{t("error_loading_settings_message")}</DimmedText>
      );
    }

    const configuration = parseNetWorthConfiguration(
      netWorthWidgetSettingsList[0]!.configuration,
    );

    if (!configuration) {
      return (
        <DimmedText size="sm">
          {t("error_loading_configuration_message")}
        </DimmedText>
      );
    }

    const netWorthWidgetGroups = configuration.groups ?? [];

    if (!netWorthWidgetGroups || netWorthWidgetGroups.length === 0) {
      return (
        <DimmedText size="sm">
          {t("widget_no_items_configured_message")}
        </DimmedText>
      );
    }

    const orderedGroups = netWorthWidgetGroups
      .slice()
      .sort((a, b) => a.index - b.index);

    return (
      <Stack gap="0.5rem">
        {orderedGroups.map((group) => {
          const sortedLines = group.lines
            .slice()
            .sort(
              (a: INetWorthWidgetLine, b: INetWorthWidgetLine) =>
                a.index - b.index,
            );

          return (
            <Card key={group.id} p="0.25rem" elevation={2}>
              <Stack gap={0}>
                {sortedLines.map((line: INetWorthWidgetLine) => (
                  <NetWorthItem
                    key={line.id}
                    title={line.name}
                    totalBalance={calculateLineTotal(
                      line,
                      validAccounts,
                      validAssets,
                      orderedGroups.flatMap((g) => g.lines),
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
    <SplitCard
      w="100%"
      h="100%"
      border={BorderThickness.Thick}
      header={
        <Group w="100%" justify="space-between">
          <Group gap="0.25rem">
            <TrendingUpIcon color="var(--base-color-text-dimmed)" />
            <PrimaryText size="xl" lh={1}>
              {t("net_worth")}
            </PrimaryText>
          </Group>
        </Group>
      }
      elevation={1}
    >
      <ScrollArea w="100%" h="100%" type="auto" offsetScrollbars="present">
        <Stack w="100%" gap="0.5rem">
          {getNetWorthLines()}
        </Stack>
      </ScrollArea>
      {settingsOpened !== undefined && onSettingsClose && (
        <NetWorthCardSettings
          widgetId={widgetId}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default NetWorthCard;
