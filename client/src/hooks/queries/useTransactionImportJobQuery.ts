import { useQuery } from "@tanstack/react-query";
import { transactionImportJobQueryKey } from "~/helpers/requests";
import { ITransactionImportJobResponse } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const transactionImportJobTerminalStatuses = [
  "Completed",
  "CompletedWithErrors",
  "Failed",
] as const;

export const useTransactionImportJobQuery = (jobId: string | null) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [transactionImportJobQueryKey, jobId],
    queryFn: async () =>
      (
        await request({
          url: `/api/transaction/import/${jobId}`,
          method: "GET",
        })
      ).data as ITransactionImportJobResponse,
    enabled: jobId !== null,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status &&
        transactionImportJobTerminalStatuses.includes(
          status as (typeof transactionImportJobTerminalStatuses)[number],
        )
        ? false
        : 1000;
    },
    refetchOnWindowFocus: true,
  });
};
