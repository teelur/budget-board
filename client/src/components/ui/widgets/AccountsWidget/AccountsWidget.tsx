import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LandmarkIcon } from "lucide-react";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import AccountsWidgetSettings from "./AccountsWidgetSettings/AccountsWidgetSettings";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";
import Divider from "~/components/core/Divider/Divider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
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
        <Flex h="100%" w="100%" p="0.5rem">
          <Skeleton flex={1} radius="md" />
        </Flex>
      );
    }

    if ((sortedFilteredInstitutionsForDisplay ?? []).length === 0) {
      return (
        <WidgetErrorMessage messageKey="widget_no_items_configured_message" />
      );
    }

    return (
      <Stack h="100%" w="100%" my="0.5rem" justify="space-around" gap={0}>
        {(sortedFilteredInstitutionsForDisplay ?? []).map(
          (institution: IInstitution, index: number) => (
            <React.Fragment key={institution.id}>
              <InstitutionItem institution={institution} />
              {index < sortedFilteredInstitutionsForDisplay.length - 1 && (
                <Divider my={"0.5rem"} size="xs" elevation={1} />
              )}
            </React.Fragment>
          ),
        )}
      </Stack>
    );
  };

  return (
    <SplitCard
      w="100%"
      h="100%"
      border={BorderThickness.Thick}
      header={
        <Group gap="0.25rem">
          <LandmarkIcon color="var(--base-color-text-dimmed)" />
          <PrimaryHeading order={3} lh={1}>
            {t("accounts")}
          </PrimaryHeading>
        </Group>
      }
      style={{
        containerType: "inline-size",
      }}
      elevation={1}
    >
      {getAccountsContent()}
      {settingsOpened !== undefined && onSettingsClose && (
        <AccountsWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default AccountsWidget;
