import { useQuery } from "@tanstack/react-query";
import { userSettingsQueryKey } from "~/helpers/requests";
import { IUserSettingsResponse } from "~/models/userSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUserSettingsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [userSettingsQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      return res.data as IUserSettingsResponse;
    },
  });
};
