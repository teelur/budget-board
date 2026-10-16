import { Group, Skeleton, Stack } from "@mantine/core";
import { BodyText, Button, Popover } from "@teelur/budget-board-ui";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import { useTranslation } from "react-i18next";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { defaultGuid } from "~/models/applicationUser";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const DisableBuiltInAccountTypes = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allAccountTypes, customAccountTypes } = useAccountTypes();
  const { disableBuiltInAccountTypes } = useUserSettings();
  const accountsQuery = useAccountsQuery();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();
  const [isConfirmationOpen, setIsConfirmationOpen] = React.useState(false);

  if (accountsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  const builtInTypeValues = new Set(
    allAccountTypes
      .filter((t) => t.id === defaultGuid)
      .map((t) => t.value.toLowerCase()),
  );

  const accountsUsingBuiltIn = (accountsQuery.data ?? []).filter((a) =>
    builtInTypeValues.has(a.type.toLowerCase()),
  );

  const customTypesWithBuiltInParent = customAccountTypes.filter(
    (t) => t.parent !== "" && builtInTypeValues.has(t.parent.toLowerCase()),
  );

  const canDisable =
    accountsUsingBuiltIn.length === 0 &&
    customTypesWithBuiltInParent.length === 0;
  const shouldConfirmDisable = !disableBuiltInAccountTypes && !canDisable;

  const blockingReasons: string[] = [];
  if (accountsUsingBuiltIn.length > 0) {
    blockingReasons.push(
      t("disable_built_in_account_types_blocked_accounts", {
        count: accountsUsingBuiltIn.length,
      }),
    );
  }
  if (customTypesWithBuiltInParent.length > 0) {
    blockingReasons.push(
      t("disable_built_in_account_types_blocked_custom_types", {
        count: customTypesWithBuiltInParent.length,
      }),
    );
  }

  const handleToggle = () => {
    updateUserSettingsMutation.mutate({
      disableBuiltInAccountTypes: !disableBuiltInAccountTypes,
    } as IUserSettingsUpdateRequest);
  };

  const toggleButton = (
    <Button
      variant="filled"
      color={disableBuiltInAccountTypes ? "error" : "primary"}
      size="xs"
      loading={updateUserSettingsMutation.isPending}
      onClick={
        shouldConfirmDisable ? () => setIsConfirmationOpen(true) : handleToggle
      }
    >
      {disableBuiltInAccountTypes ? t("disabled") : t("enabled")}
    </Button>
  );

  return (
    <Stack gap="0.25rem">
      <BodyText tone="heading" size="sm" fw={500}>
        {t("built_in_account_types")}
      </BodyText>
      <BodyText tone="secondary" size="xs">
        {t("disable_built_in_account_types_description")}
      </BodyText>
      {!canDisable &&
        blockingReasons.map((reason, i) => (
          <BodyText tone="primary" size="xs" fw={500} key={i}>
            {reason}
          </BodyText>
        ))}
      {shouldConfirmDisable ? (
        <Popover
          opened={isConfirmationOpen}
          onChange={setIsConfirmationOpen}
          position="bottom-start"
          withArrow
        >
          <Popover.Target>{toggleButton}</Popover.Target>
          <Popover.Dropdown maw={350}>
            <Stack gap="0.5rem">
              <BodyText tone="primary" size="xs">
                {t("disable_built_in_account_types_warning")}
              </BodyText>
              {blockingReasons.map((reason, i) => (
                <BodyText tone="primary" size="xs" fw={500} key={i}>
                  {reason}
                </BodyText>
              ))}
              <Group justify="flex-end" gap="0.5rem">
                <Button
                  variant="filled"
                  color="neutral"
                  size="xs"
                  flex="1 1 auto"
                  onClick={() => setIsConfirmationOpen(false)}
                >
                  {t("cancel")}
                </Button>
                <Button
                  variant="filled"
                  color="error"
                  size="xs"
                  flex="1 1 auto"
                  loading={updateUserSettingsMutation.isPending}
                  onClick={() => {
                    setIsConfirmationOpen(false);
                    handleToggle();
                  }}
                >
                  {t("confirm_disable_built_in_account_types")}
                </Button>
              </Group>
            </Stack>
          </Popover.Dropdown>
        </Popover>
      ) : (
        toggleButton
      )}
    </Stack>
  );
};

export default DisableBuiltInAccountTypes;
