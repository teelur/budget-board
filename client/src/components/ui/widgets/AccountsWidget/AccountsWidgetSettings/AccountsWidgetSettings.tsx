import { Group, ScrollArea, Skeleton, Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  Card,
  Checkbox,
  Modal,
} from "@teelur/budget-board-ui";
import React from "react";
import { parseAccountsConfiguration } from "~/helpers/widgets";
import { useTranslation } from "react-i18next";
import Divider from "~/components/core/Divider/Divider";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";

interface AccountsWidgetSettingsProps {
  widget: IWidgetSettingsResponse;
  opened: boolean;
  onClose: () => void;
}

const AccountsWidgetSettings = ({
  widget,
  opened,
  onClose,
}: AccountsWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const institutionQuery = useInstitutionsQuery();
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

  const handleClose = () => {
    setShowAll(true);
    setSelectedIds(new Set());
    setInitialized(false);
    onClose();
  };

  // We need to initialize the checkbox state from the saved configuration
  // when the user has specific accounts selected
  React.useEffect(() => {
    if (!opened || initialized) {
      return;
    }

    const config = parseAccountsConfiguration(widget?.configuration);

    if (config.accountIds.length > 0) {
      setShowAll(false);
      setSelectedIds(new Set(config.accountIds));
    } else {
      setShowAll(true);
      setSelectedIds(new Set());
    }
    setInitialized(true);
  }, [opened, initialized, widget]);

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

  const getAccountsWidgetSettingsContent = () => {
    if (institutionQuery.isPending) {
      return <Skeleton height={200} radius="lg" />;
    }

    if (visibleInstitutions.length === 0) {
      return (
        <BodyText tone="muted" size="sm" ta="center">
          {t("no_accounts_found")}
        </BodyText>
      );
    }

    return (
      <>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          selected={showAll}
          onClick={() => setShowAll((prev) => !prev)}
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
              <Card key={inst.id} p="0.5rem">
                <Stack gap="0.5rem">
                  <BodyText tone="secondary" size="sm" fw={500}>
                    {inst.name}
                  </BodyText>
                  <Stack gap="0.4rem" pl="0.25rem">
                    {inst.accounts.map((account) => (
                      <Checkbox
                        key={account.id}
                        label={account.name}
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
    <Modal opened={opened} onClose={handleClose} title={t("accounts_settings")}>
      <Stack gap="0.75rem">
        <BodyText tone="secondary" size="sm">
          {t("accounts_settings_widget_message")}
        </BodyText>
        {getAccountsWidgetSettingsContent()}
        <Group gap="0.5rem">
          <Button
            variant="filled"
            color="neutral"
            size="compact-sm"
            flex="1 1 0"
            onClick={handleClose}
          >
            {t("cancel")}
          </Button>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            flex="1 1 0"
            loading={updateWidgetSettingsMutation.isPending}
            onClick={() => {
              updateWidgetSettingsMutation.mutate([
                {
                  id: widget.id,
                  configuration: {
                    accountIds: showAll ? [] : Array.from(selectedIds),
                  },
                },
              ]);
            }}
          >
            {t("save")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default AccountsWidgetSettings;
