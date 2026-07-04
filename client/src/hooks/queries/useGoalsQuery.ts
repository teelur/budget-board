import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { goalsQueryKey } from "~/helpers/requests";
import { IGoalResponse } from "~/models/goal";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseGoalsQueryProps {
  includeInterest: boolean;
  enabled?: boolean;
}

export const useGoalsQuery = ({
  includeInterest,
  enabled = true,
}: UseGoalsQueryProps) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [goalsQueryKey, { includeInterest }],
    queryFn: async (): Promise<IGoalResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/goal",
        method: "GET",
        params: { includeInterest },
      });

      return res.data as IGoalResponse[];
    },
    enabled,
  });
};
