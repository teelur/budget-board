export interface INotificationDestinationResponse {
  id: string;
  name: string;
  eventTypes: string[];
  includeFinancialDetails: boolean;
}

export interface INotificationDestinationDetailsResponse
  extends INotificationDestinationResponse {
  url: string;
  headers: Record<string, string>;
  contentType: string;
  bodyTemplate: string;
}

export interface INotificationDestinationRequest {
  name: string;
  url?: string;
  headers?: Record<string, string>;
  contentType?: string;
  bodyTemplate?: string;
  eventTypes: string[];
  includeFinancialDetails: boolean;
}
