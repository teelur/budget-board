import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { filterVisibleAssets } from "~/helpers/assets";
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
          <Skeleton height="100%" radius="md" />
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
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default NetWorthWidget;
