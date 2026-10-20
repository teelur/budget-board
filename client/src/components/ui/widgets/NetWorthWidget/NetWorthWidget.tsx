import { Flex, Group } from "@mantine/core";
import { Card, HeadingText, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { filterVisibleAssets } from "~/helpers/assets";
import { parseNetWorthConfiguration } from "~/helpers/widgets";
import NetWorthCardSettings from "./NetWorthCardSettings/NetWorthCardSettings";
import { useTranslation } from "react-i18next";
import { TrendingUpIcon } from "lucide-react";
import WidgetErrorMessage from "../shared/WidgetErrorMessage/WidgetErrorMessage";
import NetWorthGroup from "./NetWorthGroup/NetWorthGroup";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

interface NetWorthWidgetProps {
  widget: IWidgetSettingsResponse;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const NetWorthWidget = ({
  widget,
  settingsOpened,
  onSettingsClose,
}: NetWorthWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const accountsQuery = useAccountsQuery();
  const assetsQuery = useAssetsQuery();

  const getNetWorthGroups = (): React.ReactNode => {
    if (accountsQuery.isPending || assetsQuery.isPending) {
      return (
        <Flex p="0.5rem" h="100%" w="100%">
          <Skeleton height="100%" />
        </Flex>
      );
    }

    if (!widget.configuration || widget.configuration.trim() === "") {
      return <WidgetErrorMessage messageKey="no_configuration_data_found" />;
    }

    const configuration = parseNetWorthConfiguration(widget.configuration);
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

    return orderedGroups.map((group) => (
      <Card.Section p="0.5rem" key={group.id}>
        <NetWorthGroup
          netWorthWidgetGroup={group}
          validAccounts={validAccounts}
          validAssets={validAssets}
          orderedGroups={orderedGroups}
        />
      </Card.Section>
    ));
  };

  return (
    <Card w="100%" h="100%" p={0}>
      <Card.Section separatorThickness="thick">
        <Group w="100%" justify="space-between">
          <Group gap="0.25rem">
            <TrendingUpIcon size="1.5rem" color="var(--bbui-color-primary)" />
            <HeadingText tone="heading" level={4}>
              {t("net_worth")}
            </HeadingText>
          </Group>
        </Group>
      </Card.Section>
      {getNetWorthGroups()}
      {settingsOpened !== undefined && onSettingsClose && (
        <NetWorthCardSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </Card>
  );
};

export default NetWorthWidget;
