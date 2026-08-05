import { useQuery } from "@tanstack/react-query";
import { simpleFinAccountQueryKey } from "~/helpers/requests";
import { ISimpleFinAccountResponse } from "~/models/simpleFinAccount";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useSimpleFinAccountsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [simpleFinAccountQueryKey],
    queryFn: async (): Promise<ISimpleFinAccountResponse[]> => {
      const res = await request({
        url: "/api/simpleFinAccount",
        method: "GET",
      });

      return res.data as ISimpleFinAccountResponse[];
    },
  });
};
