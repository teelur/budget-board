import { Stack } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import AddAssetType from "./AddAssetType/AddAssetType";
import DisableBuiltInAssetTypes from "./DisableBuiltInAssetTypes/DisableBuiltInAssetTypes";
import CustomAssetTypeCards from "./CustomAssetTypeCards/CustomAssetTypeCards";

const AssetTypes = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <DisableBuiltInAssetTypes />
      <Stack gap={0}>
        <BodyText tone="heading" size="sm" fw={500}>
          {t("custom_asset_types")}
        </BodyText>
        <BodyText tone="secondary" size="xs">
          {t("custom_asset_types_description")}
        </BodyText>
      </Stack>
      <AddAssetType />
      <CustomAssetTypeCards />
    </Stack>
  );
};

export default AssetTypes;
