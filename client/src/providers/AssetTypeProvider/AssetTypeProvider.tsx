import React from "react";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IAssetTypeResponse } from "~/models/assetType";
import { defaultGuid } from "~/models/applicationUser";
import { assetTypesQueryKey } from "~/helpers/requests";

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
  const { request } = useAuth();

  const assetTypesQuery = useQuery({
    queryKey: [assetTypesQueryKey],
    queryFn: async (): Promise<IAssetTypeResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/assettype",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetTypeResponse[];
      }

      return [];
    },
  });

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
