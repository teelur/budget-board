import { useQueries } from "@tanstack/react-query";
import { recurringForecastQueryKey } from "~/helpers/requests";
import { IRecurringForecastOccurrence } from "~/models/recurringRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UseRecurringForecastQueriesProps {
  months: Date[];
  enabled?: boolean;
}

export const useRecurringForecastQueries = ({
  months,
  enabled = true,
}: UseRecurringForecastQueriesProps) => {
  const { request } = useAuth();
  const { dayjs } = useLocale();

  return useQueries({
    queries: months.map((month) => {
      const monthValue = dayjs(month).format("YYYY-MM-DD");

      return {
        queryKey: [recurringForecastQueryKey, monthValue],
        queryFn: async (): Promise<IRecurringForecastOccurrence[]> => {
          const response = await request({
            url: "/api/recurringRule/forecast",
            method: "GET",
            params: { month: monthValue },
          });

          return response.data as IRecurringForecastOccurrence[];
        },
        enabled,
      };
    }),
    combine: (results) => ({
      data: results.flatMap((result) => result.data ?? []),
      isPending: results.some((result) => result.isPending),
      isError: results.some((result) => result.isError),
    }),
  });
};
