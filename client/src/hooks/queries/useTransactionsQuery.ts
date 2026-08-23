import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { transactionsQueryKey } from "~/helpers/requests";
import { ITransaction } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export interface TransactionQueryDate {
  month: number;
  year: number;
}

export interface useTransactionsQueryProps {
  selectedDates?: TransactionQueryDate[];
  includeHiddenAccounts?: boolean;
  includeHiddenCategory?: boolean;
  includeDeleted?: boolean;
  enabled?: boolean;
}

export const useTransactionsQuery = ({
  selectedDates,
  includeHiddenAccounts,
  includeHiddenCategory,
  includeDeleted,
  enabled,
}: useTransactionsQueryProps = {}) => {
  const { request } = useAuth();

  const commonParams = {
    includeHiddenAccounts: includeHiddenAccounts ?? false,
    includeHiddenCategory: includeHiddenCategory ?? false,
    includeDeleted: includeDeleted ?? false,
  };

  const createQuery = (date?: TransactionQueryDate) => {
    const params = date
      ? { ...commonParams, month: date.month, year: date.year }
      : commonParams;

    return {
      queryKey: [transactionsQueryKey, params],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params,
        });

        return res.data as ITransaction[];
      },
      enabled: enabled ?? true,
    };
  };

  const queries =
    selectedDates === undefined
      ? [createQuery()]
      : selectedDates.map((date) => createQuery(date));

  return useQueries({
    queries,
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
        isError: results.some((result) => result.isError),
        isRefetching: results.some((result) => result.isRefetching),
        refetch: () => Promise.all(results.map((result) => result.refetch())),
      };
    },
  });
};
