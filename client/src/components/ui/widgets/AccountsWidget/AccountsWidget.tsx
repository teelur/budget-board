import { Flex, Group, Skeleton } from "@mantine/core";
import { Card, HeadingText } from "@teelur/budget-board-ui";
import React from "react";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { useTranslation } from "react-i18next";
import { LandmarkIcon } from "lucide-react";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import AccountsWidgetSettings from "./AccountsWidgetSettings/AccountsWidgetSettings";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

interface AccountsWidgetProps {
  widget: IWidgetSettingsResponse;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const AccountsWidget = ({
  widget,
  settingsOpened,
  onSettingsClose,
}: AccountsWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const institutionQuery = useInstitutionsQuery();
  const widgetSettingsQuery = useWidgetSettingsQuery();

  const sortedFilteredInstitutions = React.useMemo(
    () =>
      (institutionQuery.data ?? [])
        .filter((i) => i.deleted === null)
        .sort((a, b) => a.index - b.index),
    [institutionQuery.data],
  );

  const widgetAccountIds = React.useMemo(() => {
    return parseAccountsConfiguration(widget?.configuration).accountIds;
  }, [widget?.configuration]);

  const sortedFilteredInstitutionsForDisplay = React.useMemo(
    () =>
      sortedFilteredInstitutions
        .map((inst) => ({
          ...inst,
          accounts: inst.accounts.filter(
            (a) =>
              a.deleted === null &&
              a.hideAccount === false &&
              (widgetAccountIds.length === 0 ||
                widgetAccountIds.includes(a.id)),
          ),
        }))
        .filter((inst) => inst.accounts.length > 0),
    [sortedFilteredInstitutions, widgetAccountIds],
  );

  const getAccountsContent = () => {
    if (institutionQuery.isPending || widgetSettingsQuery.isPending) {
      return (
        <Flex h="100%" w="100%" p="0.25rem">
          <Skeleton flex={1} radius="md" />
        </Flex>
      );
    }

    if ((sortedFilteredInstitutionsForDisplay ?? []).length === 0) {
      return (
        <WidgetErrorMessage messageKey="widget_no_items_configured_message" />
      );
    }

    return (sortedFilteredInstitutionsForDisplay ?? []).map(
      (institution: IInstitution) => (
        <Card.Section key={institution.id}>
          <InstitutionItem institution={institution} />
        </Card.Section>
      ),
    );
  };

  return (
    <Card
      w="100%"
      h="100%"
      p={0}
      style={{
        containerType: "inline-size",
      }}
    >
      <Card.Section>
        <Group gap="0.25rem">
          <LandmarkIcon size="1.5rem" color="var(--bbui-color-primary)" />
          <HeadingText tone="heading" level={4}>
            {t("accounts")}
          </HeadingText>
        </Group>
      </Card.Section>
      {getAccountsContent()}
      {settingsOpened !== undefined && onSettingsClose && (
        <AccountsWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </Card>
  );
};

export default AccountsWidget;
