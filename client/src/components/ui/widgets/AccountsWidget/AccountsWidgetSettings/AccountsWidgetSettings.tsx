import {
  Button,
  Checkbox,
  Group,
  ScrollArea,
  Skeleton,
  Stack,
} from "@mantine/core";
import React from "react";
import Modal from "~/components/core/Modal/Modal";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import Divider from "~/components/core/Divider/Divider";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";
import { useWidgetSettingsQuery } from "~/hooks/queries/useWidgetSettingsQuery";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";

interface AccountsWidgetSettingsProps {
  widgetId: string;
  opened: boolean;
  onClose: () => void;
}

const AccountsWidgetSettings = ({
  widgetId,
  opened,
  onClose,
}: AccountsWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const institutionQuery = useInstitutionsQuery();
  const widgetSettingsQuery = useWidgetSettingsQuery();
  const updateWidgetSettingsMutation = useUpdateWidgetSettingsMutation();

  const [showAll, setShowAll] = React.useState(true);
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
  const [initialized, setInitialized] = React.useState(false);

  // Visible accounts (not deleted, not globally hidden)
  const visibleInstitutions = React.useMemo(
    () =>
      (institutionQuery.data ?? [])
        .filter((i) => i.deleted === null)
        .sort((a, b) => a.index - b.index)
        .map((inst) => ({
          ...inst,
          accounts: inst.accounts
            .filter((a) => a.deleted === null && !a.hideAccount)
            .sort((a, b) => a.index - b.index),
        }))
        .filter((inst) => inst.accounts.length > 0),
    [institutionQuery.data],
  );

  // We need to initialize the checkbox state from the saved configuration
  // when the user has specific accounts selected
  React.useEffect(() => {
    if (!opened) {
      setInitialized(false);
      setShowAll(true);
      setSelectedIds(new Set());
      return;
    }
    if (initialized || widgetSettingsQuery.isPending) {
      return;
    }

    const widget = widgetSettingsQuery.data?.find((ws) => ws.id === widgetId);
    const config = parseAccountsConfiguration(widget?.configuration);

    if (config.accountIds.length > 0) {
      setShowAll(false);
      setSelectedIds(new Set(config.accountIds));
    } else {
      setShowAll(true);
      setSelectedIds(new Set());
    }
    setInitialized(true);
  }, [
    opened,
    initialized,
    widgetSettingsQuery.isPending,
    widgetSettingsQuery.data,
    institutionQuery.data,
    widgetId,
  ]);

  const toggle = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const isPending = widgetSettingsQuery.isPending || institutionQuery.isPending;

  const getAccountsWidgetSettingsContent = () => {
    if (isPending) {
      return <Skeleton height={200} radius="lg" />;
    }

    if (visibleInstitutions.length === 0) {
      return <DimmedText size="sm">{t("no_accounts_found")}</DimmedText>;
    }

    return (
      <>
        <Button
          variant={showAll ? "filled" : "outline"}
          onClick={() => setShowAll((prev) => !prev)}
          size="xs"
        >
          {t("show_all")}
        </Button>
        <Divider size="sm" label={t("or")} elevation={1} />
        <ScrollArea.Autosize
          mah={360}
          type="auto"
          offsetScrollbars="present"
          style={{
            opacity: showAll ? 0.4 : 1,
            pointerEvents: showAll ? "none" : undefined,
          }}
        >
          <Stack gap="0.75rem">
            {visibleInstitutions.map((inst) => (
              <Card key={inst.id} p="0.5rem" elevation={1}>
                <Stack gap="0.5rem">
                  <PrimaryText size="sm" fw={600}>
                    {inst.name}
                  </PrimaryText>
                  <Stack gap="0.4rem" pl="0.25rem">
                    {inst.accounts.map((account) => (
                      <Checkbox
                        key={account.id}
                        label={
                          <DimmedText size="sm">{account.name}</DimmedText>
                        }
                        checked={selectedIds.has(account.id)}
                        onChange={() => {
                          setShowAll(false);
                          toggle(account.id);
                        }}
                      />
                    ))}
                  </Stack>
                </Stack>
              </Card>
            ))}
          </Stack>
        </ScrollArea.Autosize>
      </>
    );
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={<PrimaryText size="md">{t("accounts_settings")}</PrimaryText>}
    >
      <Stack gap="0.5rem">
        <DimmedText size="sm">
          {t("accounts_settings_widget_message")}
        </DimmedText>
        {getAccountsWidgetSettingsContent()}
        <Group w="100%" justify="flex-end" mt="xs" gap="0.5rem">
          <Button flex={1} variant="default" onClick={onClose}>
            {t("cancel")}
          </Button>
          <Button
            flex={1}
            onClick={() => {
              updateWidgetSettingsMutation.mutate([
                {
                  id: widgetId,
                  configuration: {
                    accountIds: showAll ? [] : Array.from(selectedIds),
                  },
                },
              ]);
            }}
            loading={updateWidgetSettingsMutation.isPending}
            disabled={isPending}
          >
            {t("save")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default AccountsWidgetSettings;
