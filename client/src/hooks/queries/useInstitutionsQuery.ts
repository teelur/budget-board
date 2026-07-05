import { useQuery } from "@tanstack/react-query";
import { institutionsQueryKey } from "~/helpers/requests";
import { IInstitution } from "~/models/institution";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useInstitutionsQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [institutionsQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/institution",
        method: "GET",
      });

      return res.data as IInstitution[];
    },
  });
};
