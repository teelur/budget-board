import { useQuery } from "@tanstack/react-query";
import { notificationDestinationsQueryKey } from "~/helpers/requests";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useNotificationDestinationsQuery = () => {
  const { request } = useAuth();
  return useQuery({
    queryKey: [notificationDestinationsQueryKey],
    queryFn: async (): Promise<INotificationDestinationResponse[]> =>
      (await request({ url: "/api/NotificationDestination", method: "GET" }))
        .data,
  });
};
