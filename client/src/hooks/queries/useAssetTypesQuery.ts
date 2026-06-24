import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { assetTypesQueryKey } from "~/helpers/requests";
import { IAccountTypeResponse } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAssetTypesQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [assetTypesQueryKey],
    queryFn: async (): Promise<IAccountTypeResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/assetType",
        method: "GET",
      });

      return res.data as IAccountTypeResponse[];
    },
    meta: {
      skipGlobalErrorToast: true,
    },
  });
};
