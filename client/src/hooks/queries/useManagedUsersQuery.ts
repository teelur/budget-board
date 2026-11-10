import { useQuery } from "@tanstack/react-query";
import { managedUsersQueryKey } from "~/helpers/requests";
import { IManagedUserResponse } from "~/models/managedUser";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useManagedUsersQuery = (enabled: boolean) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [managedUsersQueryKey],
    enabled,
    queryFn: async (): Promise<IManagedUserResponse[]> => {
      const response = await request({
        url: "/api/admin/users",
        method: "GET",
      });

      return response.data as IManagedUserResponse[];
    },
  });
};
