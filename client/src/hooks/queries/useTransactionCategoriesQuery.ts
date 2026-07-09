import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { transactionCategoriesQueryKey } from "~/helpers/requests";
import { ICategoryResponse } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useTransactionCategoriesQuery = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [transactionCategoriesQueryKey],
    queryFn: async (): Promise<ICategoryResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      return res.data as ICategoryResponse[];
    },
  });
};
