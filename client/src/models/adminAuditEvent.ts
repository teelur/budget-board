export interface IAdminAuditEventResponse {
  id: number;
  actorUserId: string | null;
  actorUserName: string | null;
  targetUserId: string | null;
  targetUserName: string | null;
  action: string;
  succeeded: boolean;
  createdAt: string;
  requestIpAddress: string | null;
  details: string | null;
}

export interface IAdminAuditEventsPageResponse {
  events: IAdminAuditEventResponse[];
  actions: string[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface IAdminAuditEventsQuery {
  page: number;
  pageSize: number;
  action?: string;
  succeeded?: boolean;
  userName?: string;
}
