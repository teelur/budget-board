import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { twoFactorAuthQueryKey } from "~/helpers/requests";
import { TwoFactorAuthResponse } from "~/models/twoFactorAuth";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useTwoFactorAuthenticationQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [twoFactorAuthQueryKey],
    queryFn: async (): Promise<TwoFactorAuthResponse | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/manage/2fa",
        method: "GET",
      });

      return res.data as TwoFactorAuthResponse;
    },
  });
};
