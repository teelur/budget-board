import { useQuery } from "@tanstack/react-query";
import { simpleFinOrganizationQueryKey } from "~/helpers/requests";
import { ISimpleFinOrganizationResponse } from "~/models/simpleFinOrganization";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useSimpleFinOrganizationsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [simpleFinOrganizationQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/simpleFinOrganization",
        method: "GET",
      });

      return res.data as ISimpleFinOrganizationResponse[];
    },
  });
};
