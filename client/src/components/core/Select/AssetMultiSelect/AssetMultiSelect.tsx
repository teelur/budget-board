import React from "react";
import { MultiSelect, MultiSelectProps } from "@teelur/budget-board-ui";
import { IAssetResponse } from "~/models/asset";
import { useTranslation } from "react-i18next";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

export interface AssetMultiSelectProps extends MultiSelectProps {
  selectedAssetIds?: string[];
  setSelectedAssetIds?: (assetIds: string[]) => void;
  hideHidden?: boolean;
  maxSelectedValues?: number;
}

const AssetMultiSelect = ({
  selectedAssetIds,
  setSelectedAssetIds,
  hideHidden = false,
  maxSelectedValues = undefined,
  ...props
}: AssetMultiSelectProps): React.ReactNode => {
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

export default AssetMultiSelect;
