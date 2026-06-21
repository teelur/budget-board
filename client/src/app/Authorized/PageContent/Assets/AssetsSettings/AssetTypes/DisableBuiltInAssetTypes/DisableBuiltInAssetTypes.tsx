import { Button, Skeleton, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  assetTypesQueryKey,
  translateAxiosError,
  userSettingsQueryKey,
} from "~/helpers/requests";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { defaultGuid } from "~/models/applicationUser";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

const DisableBuiltInAssetTypes = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const { allAssetTypes, customAssetTypes } = useAssetTypes();
  const { disableBuiltInAssetTypes } = useUserSettings();
  const assetsQuery = useAssetsQuery();

  const queryClient = useQueryClient();
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [userSettingsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [assetTypesQueryKey] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

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
      <Button
        bg={disableBuiltInAssetTypes ? "var(--button-color-destructive)" : ""}
        variant="primary"
        size="xs"
        disabled={!disableBuiltInAssetTypes && !canDisable}
        loading={doUpdateUserSettings.isPending}
        onClick={() => {
          doUpdateUserSettings.mutate({
            disableBuiltInAssetTypes: !disableBuiltInAssetTypes,
          } as IUserSettingsUpdateRequest);
        }}
      >
        {disableBuiltInAssetTypes ? t("disabled") : t("enabled")}
      </Button>
    </Stack>
  );
};

export default DisableBuiltInAssetTypes;
