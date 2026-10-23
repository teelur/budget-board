import { Group, Stack } from "@mantine/core";
import { BodyText, Button, Popover, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import { useTranslation } from "react-i18next";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { defaultGuid } from "~/models/applicationUser";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const DisableBuiltInAssetTypes = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allAssetTypes, customAssetTypes } = useAssetTypes();
  const { disableBuiltInAssetTypes } = useUserSettings();
  const assetsQuery = useAssetsQuery();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();
  const [isConfirmationOpen, setIsConfirmationOpen] = React.useState(false);

  if (assetsQuery.isPending) {
    return <Skeleton height={75} />;
  }

  const builtInTypeValues = new Set(
    allAssetTypes
      .filter((t) => t.id === defaultGuid)
      .map((t) => t.value.toLowerCase()),
  );

  const assetsUsingBuiltIn = (assetsQuery.data ?? []).filter(
    (a) => a.type && builtInTypeValues.has(a.type.toLowerCase()),
  );

  const customTypesWithBuiltInParent = customAssetTypes.filter(
    (t) => t.parent !== "" && builtInTypeValues.has(t.parent.toLowerCase()),
  );

  const canDisable =
    assetsUsingBuiltIn.length === 0 &&
    customTypesWithBuiltInParent.length === 0;
  const shouldConfirmDisable = !disableBuiltInAssetTypes && !canDisable;

  const blockingReasons: string[] = [];
  if (assetsUsingBuiltIn.length > 0) {
    blockingReasons.push(
      t("disable_built_in_asset_types_blocked_assets", {
        count: assetsUsingBuiltIn.length,
      }),
    );
  }
  if (customTypesWithBuiltInParent.length > 0) {
    blockingReasons.push(
      t("disable_built_in_asset_types_blocked_custom_types", {
        count: customTypesWithBuiltInParent.length,
      }),
    );
  }

  const handleToggle = () => {
    updateUserSettingsMutation.mutate({
      disableBuiltInAssetTypes: !disableBuiltInAssetTypes,
    } as IUserSettingsUpdateRequest);
  };

  const toggleButton = (
    <Button
      variant="filled"
      color={disableBuiltInAssetTypes ? "error" : "primary"}
      size="xs"
      loading={updateUserSettingsMutation.isPending}
      onClick={
        shouldConfirmDisable ? () => setIsConfirmationOpen(true) : handleToggle
      }
    >
      {disableBuiltInAssetTypes ? t("disabled") : t("enabled")}
    </Button>
  );

  return (
    <Stack gap="0.25rem">
      <BodyText tone="heading" size="sm" fw={500}>
        {t("built_in_asset_types")}
      </BodyText>
      <BodyText tone="secondary" size="xs">
        {t("disable_built_in_asset_types_description")}
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
                {t("disable_built_in_asset_types_warning")}
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
                  {t("confirm_disable_built_in_asset_types")}
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

export default DisableBuiltInAssetTypes;
