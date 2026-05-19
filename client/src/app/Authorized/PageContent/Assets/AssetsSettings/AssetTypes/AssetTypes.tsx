import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import AddAssetType from "./AddAssetType/AddAssetType";
import DisableBuiltInAssetTypes from "./DisableBuiltInAssetTypes/DisableBuiltInAssetTypes";
import CustomAssetTypeCards from "./CustomAssetTypeCards/CustomAssetTypeCards";

const AssetTypes = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <DisableBuiltInAssetTypes />
      <Stack gap="0.25rem">
        <PrimaryText size="sm">{t("custom_asset_types")}</PrimaryText>
        <DimmedText size="xs">{t("custom_asset_types_description")}</DimmedText>
      </Stack>
      <AddAssetType />
      <CustomAssetTypeCards />
    </Stack>
  );
};

export default AssetTypes;
