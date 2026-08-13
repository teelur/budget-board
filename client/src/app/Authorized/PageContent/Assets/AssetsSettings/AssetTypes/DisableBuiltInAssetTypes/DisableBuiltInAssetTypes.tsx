import {
  Button,
  Group,
  Popover as MantinePopover,
  Skeleton,
  Stack,
} from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Popover from "~/components/core/Popover/Popover";
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
    return <Skeleton height={75} radius="md" />;
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
      bg={disableBuiltInAssetTypes ? "var(--button-color-destructive)" : ""}
      variant="primary"
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
      <PrimaryText size="sm">{t("built_in_asset_types")}</PrimaryText>
      <DimmedText size="xs">
        {t("disable_built_in_asset_types_description")}
      </DimmedText>
      {!canDisable &&
        blockingReasons.map((reason, i) => (
          <PrimaryText key={i} size="xs">
            {reason}
          </PrimaryText>
        ))}
      {shouldConfirmDisable ? (
        <Popover
          opened={isConfirmationOpen}
          onChange={setIsConfirmationOpen}
          position="bottom-start"
          withArrow
        >
          <MantinePopover.Target>{toggleButton}</MantinePopover.Target>
          <MantinePopover.Dropdown maw={350}>
            <Stack gap="0.5rem">
              <PrimaryText size="xs">
                {t("disable_built_in_asset_types_warning")}
              </PrimaryText>
              {blockingReasons.map((reason, i) => (
                <PrimaryText key={i} size="xs">
                  {reason}
                </PrimaryText>
              ))}
              <Group justify="flex-end" gap="0.5rem">
                <Button
                  variant="subtle"
                  size="xs"
                  onClick={() => setIsConfirmationOpen(false)}
                >
                  {t("cancel")}
                </Button>
                <Button
                  color="var(--button-color-destructive)"
                  size="xs"
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
          </MantinePopover.Dropdown>
        </Popover>
      ) : (
        toggleButton
      )}
    </Stack>
  );
};

export default DisableBuiltInAssetTypes;
