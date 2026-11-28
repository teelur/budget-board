export type NotificationDestinationProvider = "webhook" | "ntfy" | "discord";

export interface INotificationDestinationResponse {
  id: string;
  name: string;
  provider: NotificationDestinationProvider;
  enabled: boolean;
  eventTypes: string[];
  includeFinancialDetails: boolean;
}

export type INotificationDestinationDetailsResponse =
  INotificationDestinationResponse &
    (
      | {
          provider: "webhook";
          url: string;
          headers: Record<string, string>;
          contentType: string;
          bodyTemplate: string;
        }
      | {
          provider: "ntfy";
          ntfyServerUrl: string;
          ntfyTopic: string;
          ntfyAccessToken: string | null;
        }
      | {
          provider: "discord";
          discordWebhookUrl: string;
        }
    );

interface NotificationDestinationRequestBase {
  name: string;
  eventTypes: string[];
  includeFinancialDetails: boolean;
}

export type INotificationDestinationRequest =
  NotificationDestinationRequestBase &
    (
      | {
          provider: "webhook";
          url: string;
          headers?: Record<string, string>;
          contentType: string;
          bodyTemplate?: string;
        }
      | {
          provider: "ntfy";
          ntfyServerUrl: string;
          ntfyTopic: string;
          ntfyAccessToken: string;
        }
      | {
          provider: "discord";
          discordWebhookUrl: string;
        }
    );
