import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { balancesQueryKey } from "~/helpers/requests";
import { IBalanceResponse } from "~/models/balance";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseBalancesQueryProps {
  accountIds: string[];
  enabled?: boolean;
}

export const useBalancesQuery = ({
  accountIds,
  enabled = true,
}: UseBalancesQueryProps) => {
  const { request } = useAuth();

  return useQueries({
    queries: accountIds.map((accountId) => ({
      queryKey: [balancesQueryKey, accountId],
      queryFn: async (): Promise<IBalanceResponse[]> => {
        const res: AxiosResponse = await request({
          url: "/api/balance",
          method: "GET",
          params: { accountId },
        });

        return res.data as IBalanceResponse[];
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
