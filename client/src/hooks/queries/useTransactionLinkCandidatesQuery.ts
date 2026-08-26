import { useQuery } from "@tanstack/react-query";
import { transactionLinkCandidatesQueryKey } from "~/helpers/requests";
import { ITransaction } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useTransactionLinkCandidatesQuery = (
  transactionID: string,
  enabled: boolean,
  dateWindowDays: number,
) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [
      transactionLinkCandidatesQueryKey,
      transactionID,
      dateWindowDays,
    ],
    queryFn: async (): Promise<ITransaction[]> => {
      const response = await request({
        url: `/api/transaction/link-candidates/${transactionID}`,
        method: "GET",
        params: { dateWindowDays },
      });

      return response.data as ITransaction[];
    },
    enabled,
  });
};
