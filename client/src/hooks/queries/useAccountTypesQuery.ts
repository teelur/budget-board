import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { accountTypesQueryKey } from "~/helpers/requests";
import { IAccountTypeResponse } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAccountTypesQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [accountTypesQueryKey],
    queryFn: async (): Promise<IAccountTypeResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/accountType",
        method: "GET",
      });

      return res.data as IAccountTypeResponse[];
    },
  });
};
