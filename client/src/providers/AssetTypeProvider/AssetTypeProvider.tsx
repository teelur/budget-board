import React from "react";
import { IAssetTypeResponse } from "~/models/assetType";
import { defaultGuid } from "~/models/applicationUser";
import { useAssetTypesQuery } from "~/hooks/queries/useAssetTypesQuery";

interface AssetTypeContextType {
  allAssetTypes: IAssetTypeResponse[];
  customAssetTypes: IAssetTypeResponse[];
}

export const AssetTypeContext = React.createContext<AssetTypeContextType>({
  allAssetTypes: [],
  customAssetTypes: [],
});

interface AssetTypeProviderProps {
  children: React.ReactNode;
}

export const AssetTypeProvider = (props: AssetTypeProviderProps) => {
  const assetTypesQuery = useAssetTypesQuery();

  const customAssetTypes = React.useMemo(
    () =>
      assetTypesQuery.data
        ? assetTypesQuery.data.filter((type) => type.id !== defaultGuid)
        : [],
    [assetTypesQuery.data],
  );

  const value = React.useMemo(
    () => ({
      allAssetTypes: assetTypesQuery.data ?? [],
      customAssetTypes: customAssetTypes,
    }),
    [assetTypesQuery.data, customAssetTypes],
  );

  return (
    <AssetTypeContext.Provider value={value}>
      {props.children}
    </AssetTypeContext.Provider>
  );
};

export const useAssetTypes = () => React.useContext(AssetTypeContext);
