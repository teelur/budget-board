import { useQuery } from "@tanstack/react-query";
import { lunchFlowAccountQueryKey } from "~/helpers/requests";
import { ILunchFlowAccountResponse } from "~/models/lunchFlowAccount";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useLunchFlowAccountsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [lunchFlowAccountQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/lunchFlowAccount",
        method: "GET",
      });

      return res.data as ILunchFlowAccountResponse[];
    },
  });
};
