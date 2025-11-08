import { MultiSelect } from "@mantine/core";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IAssetResponse } from "~/models/asset";

interface AssetSelectInputProps {
  selectedAssetIds?: string[];
  setSelectedAssetIds?: (assetIds: string[]) => void;
  hideHidden?: boolean;
  maxSelectedValues?: number;
  [x: string]: any;
}

const AssetSelectInput = ({
  selectedAssetIds,
  setSelectedAssetIds,
  hideHidden = false,
  maxSelectedValues = undefined,
  ...props
}: AssetSelectInputProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
      placeholder="Select assets"
      value={selectedAssetIds}
      onChange={setSelectedAssetIds}
      clearable
      maxValues={maxSelectedValues}
      {...props}
    />
  );
};

export default AssetSelectInput;
