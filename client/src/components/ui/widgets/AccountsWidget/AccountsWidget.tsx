import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LandmarkIcon } from "lucide-react";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import AccountsWidgetSettings from "./AccountsWidgetSettings/AccountsWidgetSettings";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";
import Divider from "~/components/core/Divider/Divider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";

interface AccountsWidgetProps {
  widgetId: string;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const AccountsWidget = ({
  widgetId,
  settingsOpened,
  onSettingsClose,
}: AccountsWidgetProps): React.ReactNode => {
  const { t } = useTranslation();

  const { request } = useAuth();
  const institutionQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async (): Promise<IInstitution[]> => {
      const res: AxiosResponse = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IInstitution[];
      }

      return [];
    },
  });

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

  const sortedFilteredInstitutions = React.useMemo(
    () =>
      (institutionQuery.data ?? [])
        .filter((i) => i.deleted === null)
        .sort((a, b) => a.index - b.index),
    [institutionQuery.data],
  );

  const widgetAccountIds = React.useMemo(() => {
    const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
    return parseAccountsConfiguration(widget?.configuration).accountIds;
  }, [widgetSettingsQuery.data, widgetId]);

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
          widgetId={widgetId}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </SplitCard>
  );
};

export default AccountsWidget;
