import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { widgetSettingsQueryKey } from "~/helpers/requests";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useWidgetSettingsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [widgetSettingsQueryKey],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });

      return res.data as IWidgetSettingsResponse[];
    },
  });
};
