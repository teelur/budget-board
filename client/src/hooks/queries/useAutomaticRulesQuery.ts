import { useQuery } from "@tanstack/react-query";
import { automaticRulesQueryKey } from "~/helpers/requests";
import { IAutomaticRuleResponse } from "~/models/automaticRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAutomaticRulesQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [automaticRulesQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/automaticRule",
        method: "GET",
      });

      return res.data as IAutomaticRuleResponse[];
    },
  });
};
