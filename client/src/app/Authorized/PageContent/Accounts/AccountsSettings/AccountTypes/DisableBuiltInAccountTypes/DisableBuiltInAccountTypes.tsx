import { Button, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
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

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">{t("built_in_account_types")}</PrimaryText>
      <DimmedText size="xs">
        {t("disable_built_in_account_types_description")}
      </DimmedText>
      {!canDisable &&
        blockingReasons.map((reason, i) => (
          <PrimaryText key={i} size="xs">
            {reason}
          </PrimaryText>
        ))}
      <Button
        bg={disableBuiltInAccountTypes ? "var(--button-color-destructive)" : ""}
        variant="primary"
        size="xs"
        disabled={!disableBuiltInAccountTypes && !canDisable}
        loading={updateUserSettingsMutation.isPending}
        onClick={() => {
          updateUserSettingsMutation.mutate({
            disableBuiltInAccountTypes: !disableBuiltInAccountTypes,
          } as IUserSettingsUpdateRequest);
        }}
      >
        {disableBuiltInAccountTypes ? t("disabled") : t("enabled")}
      </Button>
    </Stack>
  );
};

export default DisableBuiltInAccountTypes;
