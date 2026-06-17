import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { applicationUserQueryKey } from "~/helpers/requests";
import { IApplicationUserResponse } from "~/models/applicationUser";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useApplicationUserQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [applicationUserQueryKey],
    queryFn: async (): Promise<IApplicationUserResponse> => {
      const res: AxiosResponse = await request({
        url: "/api/applicationUser",
        method: "GET",
      });

      return res.data as IApplicationUserResponse;
    },
  });
};
