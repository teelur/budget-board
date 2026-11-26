import { useQuery } from "@tanstack/react-query";
import { notificationDestinationsQueryKey } from "~/helpers/requests";
import type { INotificationDestinationDetailsResponse } from "~/models/notificationDestination";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useNotificationDestinationDetailsQuery = (id: string) => {
  const { request } = useAuth();
  return useQuery({
    queryKey: [notificationDestinationsQueryKey, id],
    queryFn: async (): Promise<INotificationDestinationDetailsResponse> => {
      return (
        await request({
          url: `/api/NotificationDestination/${id}`,
          method: "GET",
        })
      ).data;
    },
  });
};
