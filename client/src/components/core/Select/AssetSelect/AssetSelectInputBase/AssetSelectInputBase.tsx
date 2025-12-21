import { MultiSelect, MultiSelectProps } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IAssetResponse } from "~/models/asset";
import { useTranslation } from "react-i18next";

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
  const { request } = useAuth();

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return [];
    },
  });

  const getFilteredAssets = (): IAssetResponse[] => {
    let filteredAssets = (assetsQuery.data ?? []).filter(
      (a) => a.deleted === null
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
