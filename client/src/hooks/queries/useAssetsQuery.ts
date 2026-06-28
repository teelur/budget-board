import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { assetsQueryKey } from "~/helpers/requests";
import { IAssetResponse } from "~/models/asset";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseAssetsQueryProps {
  enabled?: boolean;
}

export const useAssetsQuery = ({
  enabled = true,
}: UseAssetsQueryProps = {}) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [assetsQueryKey],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      return res.data as IAssetResponse[];
    },
    enabled,
  });
};
