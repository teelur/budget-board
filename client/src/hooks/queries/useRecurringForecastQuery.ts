import { useQuery } from "@tanstack/react-query";
import { recurringForecastQueryKey } from "~/helpers/requests";
import { IRecurringForecastOccurrence } from "~/models/recurringRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UseRecurringForecastQueryProps {
  month: Date | null;
  enabled?: boolean;
}

export const useRecurringForecastQuery = ({
  month,
  enabled = true,
}: UseRecurringForecastQueryProps) => {
  const { request } = useAuth();
  const { dayjs } = useLocale();
  const monthValue = month ? dayjs(month).format("YYYY-MM-DD") : null;

  return useQuery({
    queryKey: [recurringForecastQueryKey, monthValue],
    queryFn: async (): Promise<IRecurringForecastOccurrence[]> => {
      const response = await request({
        url: "/api/recurringRule/forecast",
        method: "GET",
        params: { month: monthValue },
      });

      return response.data as IRecurringForecastOccurrence[];
    },
    enabled: enabled && monthValue !== null,
  });
};
