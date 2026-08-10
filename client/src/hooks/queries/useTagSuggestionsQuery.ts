import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { tagSuggestionsQueryKey } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export interface UseTagSuggestionsQueryProps {
  prefix?: string;
  limit?: number;
  enabled?: boolean;
}

export const useTagSuggestionsQuery = ({
  prefix = "",
  limit = 20,
  enabled = true,
}: UseTagSuggestionsQueryProps = {}) => {
  const { request } = useAuth();
  const trimmedPrefix = prefix.trim();

  return useQuery({
    queryKey: [tagSuggestionsQueryKey, { prefix: trimmedPrefix, limit }],
    queryFn: async (): Promise<string[]> => {
      const response: AxiosResponse = await request({
        url: "/api/tag/suggestions",
        method: "GET",
        params: {
          prefix: trimmedPrefix || undefined,
          limit,
        },
      });

      return response.data as string[];
    },
    enabled,
  });
};
