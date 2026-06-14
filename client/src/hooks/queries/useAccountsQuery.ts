import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { accountsQueryKey } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAccountsQuery = ({
  enabled = true,
}: { enabled?: boolean } = {}) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [accountsQueryKey],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      return res.data as IAccountResponse[];
    },
    enabled,
  });
};
