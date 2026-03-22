import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { ICategory } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

const useTransactionCategories = () => {
  const { request } = useAuth();

  return useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async (): Promise<ICategory[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategory[];
      }

      return [];
    },
  });
};

export default useTransactionCategories;
