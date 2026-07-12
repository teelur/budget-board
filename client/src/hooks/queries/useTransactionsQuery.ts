import { useQueries, useQuery } from "@tanstack/react-query";
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
  includeHidden?: boolean;
  includeDeleted?: boolean;
  enabled?: boolean;
}

export const useTransactionsQuery = ({
  selectedDates,
  includeHidden,
  includeDeleted,
  enabled,
}: useTransactionsQueryProps = {}) => {
  const { request } = useAuth();

  // TODO: We should move towards querying by month, since querying EVERYTHING will get very cumbersome once the data
  // set grows large.
  if (!selectedDates || selectedDates.length === 0) {
    return useQuery({
      queryKey: [
        transactionsQueryKey,
        {
          includeHidden: includeHidden ?? false,
          includeDeleted: includeDeleted ?? false,
        },
      ],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: {
            includeHidden: includeHidden ?? false,
            includeDeleted: includeDeleted ?? false,
          },
        });

        return res.data as ITransaction[];
      },
      enabled: enabled ?? true,
    });
  }

  return useQueries({
    queries: selectedDates.map((date: TransactionQueryDate) => ({
      queryKey: [
        transactionsQueryKey,
        {
          month: date.month,
          year: date.year,
          includeHidden: includeHidden ?? false,
          includeDeleted: includeDeleted ?? false,
        },
      ],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: {
            month: date.month,
            year: date.year,
            includeHidden: includeHidden ?? false,
            includeDeleted: includeDeleted ?? false,
          },
        });

        return res.data as ITransaction[];
      },
      enabled: enabled ?? true,
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });
};
