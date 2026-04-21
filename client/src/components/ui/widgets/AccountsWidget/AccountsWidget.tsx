import { Group, ScrollArea, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItems/InstitutionItem";
import { IAccountResponse } from "~/models/account";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LandmarkIcon } from "lucide-react";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import AccountsWidgetSettings from "./AccountsWidgetSettings/AccountsWidgetSettings";
import WidgetErrorMessage from "~/components/ui/widgets/shared/WidgetErrorMessage/WidgetErrorMessage";

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
    if (institutionQuery.isPending || accountsQuery.isPending) {
      return <Skeleton height="100%" radius="md" />;
    }

    if ((sortedFilteredInstitutionsForDisplay ?? []).length === 0) {
      return (
        <WidgetErrorMessage messageKey="widget_no_items_configured_message" />
      );
    }

    return (
      <ScrollArea w="100%" h="100%" type="auto" offsetScrollbars="present">
        <Stack align="center" gap="0.5rem">
          {(sortedFilteredInstitutionsForDisplay ?? []).map(
            (institution: IInstitution) => (
              <InstitutionItem key={institution.id} institution={institution} />
            ),
          )}
        </Stack>
      </ScrollArea>
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
          <PrimaryText size="xl" lh={1}>
            {t("accounts")}
          </PrimaryText>
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
