import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { valuesQueryKey } from "~/helpers/requests";
import { IValueResponse } from "~/models/value";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseValuesQueryProps {
  assetIds: string[];
  enabled?: boolean;
}

export const useValuesQuery = ({
  assetIds,
  enabled = true,
}: UseValuesQueryProps) => {
  const { request } = useAuth();

  return useQueries({
    queries: assetIds.map((assetId: string) => ({
      queryKey: [valuesQueryKey, assetId],
      queryFn: async (): Promise<IValueResponse[]> => {
        const res: AxiosResponse = await request({
          url: "/api/value",
          method: "GET",
          params: { assetId },
        });

        return res.data as IValueResponse[];
      },
      enabled,
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });
};
