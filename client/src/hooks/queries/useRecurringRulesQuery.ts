import { useQuery } from "@tanstack/react-query";
import { recurringRulesQueryKey } from "~/helpers/requests";
import { IRecurringRuleResponse } from "~/models/recurringRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useRecurringRulesQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [recurringRulesQueryKey],
    queryFn: async (): Promise<IRecurringRuleResponse[]> => {
      const response = await request({
        url: "/api/recurringRule",
        method: "GET",
      });

      return response.data as IRecurringRuleResponse[];
    },
  });
};