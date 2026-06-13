import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IAccountResponse } from "~/models/account";
import { AxiosResponse } from "axios";
import { filterVisibleAssets } from "~/helpers/assets";
import { IAssetResponse } from "~/models/asset";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { parseNetWorthConfiguration } from "~/helpers/widgets";
import NetWorthCardSettings from "./NetWorthCardSettings/NetWorthCardSettings";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { TrendingUpIcon } from "lucide-react";
import WidgetErrorMessage from "../shared/WidgetErrorMessage/WidgetErrorMessage";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import NetWorthGroup from "./NetWorthGroup/NetWorthGroup";
import Divider from "~/components/core/Divider/Divider";
import { accountsQueryKey, assetsQueryKey, widgetSettingsQueryKey } from "~/helpers/requests";


interface NetWorthWidgetProps {
  widgetId: string;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const NetWorthWidget = ({
  widgetId,
  settingsOpened,
  onSettingsClose,
}: NetWorthWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const widgetSettingsQuery = useQuery({
    queryKey: [widgetSettingsQueryKey],
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
    queryKey: [accountsQueryKey],
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
    queryKey: [assetsQueryKey],
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

  const getNetWorthGroups = (): React.ReactNode => {
    if (
      widgetSettingsQuery.isPending ||
      accountsQuery.isPending ||
      assetsQuery.isPending
    ) {
      return (
        <Flex p="0.5rem" h="100%" w="100%">
          <Skeleton height="100%" radius="md" />
        </Flex>
      );
    }

    if (!widgetSettingsQuery.data || widgetSettingsQuery.data.length === 0) {
      return <WidgetErrorMessage messageKey="no_configuration_data_found" />;
    }

    const netWorthWidgetSettingsList = widgetSettingsQuery.data
      .slice()
      .filter((widget) => widget.id === widgetId);

    if (netWorthWidgetSettingsList.length === 0) {
      return <WidgetErrorMessage messageKey="error_loading_settings_message" />;
    }

    const configuration = parseNetWorthConfiguration(
      netWorthWidgetSettingsList[0]!.configuration,
    );

    if (!configuration) {
      return (
        <WidgetErrorMessage messageKey="error_loading_configuration_message" />
      );
    }

    const netWorthWidgetGroups = configuration.groups ?? [];

    if (!netWorthWidgetGroups || netWorthWidgetGroups.length === 0) {
      return (
        <WidgetErrorMessage messageKey="widget_no_items_configured_message" />
      );
    }

    const orderedGroups = netWorthWidgetGroups
      .slice()
      .sort((a, b) => a.index - b.index);

    const validAccounts = filterVisibleAccounts(accountsQuery.data ?? []);
    const validAssets = filterVisibleAssets(assetsQuery.data ?? []);

    return (
      <Stack h="100%" w="100%" my="0.5rem" justify="space-around" gap={0}>
        {orderedGroups.map((group, index) => (
          <React.Fragment key={group.id}>
            <NetWorthGroup
              netWorthWidgetGroup={group}
              validAccounts={validAccounts}
              validAssets={validAssets}
              orderedGroups={orderedGroups}
            />
            {index < orderedGroups.length - 1 && (
              <Divider my={"0.5rem"} size="xs" elevation={1} />
            )}
          </React.Fragment>
        ))}
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
            <PrimaryHeading order={3} lh={1}>
              {t("net_worth")}
            </PrimaryHeading>
          </Group>
        </Group>
      }
      elevation={1}
    >
      {getNetWorthGroups()}
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

export default NetWorthWidget;