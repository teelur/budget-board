import { MultiSelect, MultiSelectProps } from "@mantine/core";
import React from "react";
import { IAssetResponse } from "~/models/asset";
import { useTranslation } from "react-i18next";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

export interface AssetSelectInputBaseProps extends MultiSelectProps {
  selectedAssetIds?: string[];
  setSelectedAssetIds?: (assetIds: string[]) => void;
  hideHidden?: boolean;
  maxSelectedValues?: number;
}

const AssetSelectInputBase = ({
  selectedAssetIds,
  setSelectedAssetIds,
  hideHidden = false,
  maxSelectedValues = undefined,
  ...props
}: AssetSelectInputBaseProps): React.ReactNode => {
  const { t } = useTranslation();
  const assetsQuery = useAssetsQuery();

  const getFilteredAssets = (): IAssetResponse[] => {
    let filteredAssets = (assetsQuery.data ?? []).filter(
      (a) => a.deleted === null,
    );

    if (hideHidden) {
      filteredAssets = filteredAssets.filter((a) => !a.hide);
    }

    return filteredAssets;
  };

  return (
    <MultiSelect
      data={getFilteredAssets().map((a) => {
        return { value: a.id, label: a.name };
      })}
      placeholder={t("select_assets")}
      value={selectedAssetIds}
      onChange={setSelectedAssetIds}
      clearable
      maxValues={maxSelectedValues}
      {...props}
    />
  );
};

export default AssetSelectInputBase;
