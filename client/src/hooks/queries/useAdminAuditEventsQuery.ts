import { useQuery } from "@tanstack/react-query";
import { adminAuditEventsQueryKey } from "~/helpers/requests";
import type {
  IAdminAuditEventsPageResponse,
  IAdminAuditEventsQuery,
} from "~/models/adminAuditEvent";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseAdminAuditEventsQueryOptions extends IAdminAuditEventsQuery {
  enabled: boolean;
}

export const useAdminAuditEventsQuery = ({
  enabled,
  ...query
}: UseAdminAuditEventsQueryOptions) => {
  const { request } = useAuth();

  return useQuery({
    queryKey: [adminAuditEventsQueryKey, query],
    enabled,
    queryFn: async (): Promise<IAdminAuditEventsPageResponse> => {
      const params = new URLSearchParams({
        page: String(query.page),
        pageSize: String(query.pageSize),
      });

      if (query.action) {
        params.set("action", query.action);
      }

      if (query.succeeded !== undefined) {
        params.set("succeeded", String(query.succeeded));
      }

      if (query.userName) {
        params.set("userName", query.userName);
      }

      const response = await request({
        url: `/api/admin/audit-events?${params.toString()}`,
        method: "GET",
      });

      return response.data as IAdminAuditEventsPageResponse;
    },
  });
};
