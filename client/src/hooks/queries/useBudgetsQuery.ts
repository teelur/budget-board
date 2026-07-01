import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { budgetsQueryKey } from "~/helpers/requests";
import { IBudget } from "~/models/budget";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UseBudgetsQueryProps {
  months: Date[];
  enabled?: boolean;
}

export const useBudgetsQuery = ({
  months,
  enabled = true,
}: UseBudgetsQueryProps) => {
  const { request } = useAuth();
  const { dayjs } = useLocale();

  return useQueries({
    queries: months.map((month: Date) => ({
      queryKey: [budgetsQueryKey, dayjs(month).format("YYYY-MM")],
      queryFn: async (): Promise<IBudget[]> => {
        const res: AxiosResponse = await request({
          url: "/api/budget",
          method: "GET",
          params: { month: dayjs(month).format("YYYY-MM-DD") },
        });

        return res.data as IBudget[];
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
