import { notifications, type NotificationData } from "@mantine/notifications";

export enum NotificationType {
  Error = "error",
  Success = "success",
  Warning = "warning",
  Information = "information",
}

type NotificationSemanticColor = "error" | "success" | "warning" | "info";

const notificationColors: Record<NotificationType, NotificationSemanticColor> =
  {
    [NotificationType.Error]: "error",
    [NotificationType.Success]: "success",
    [NotificationType.Warning]: "warning",
    [NotificationType.Information]: "info",
  };

export interface INotificationOptions extends Omit<NotificationData, "color"> {
  type: NotificationType;
}

export const showNotification = ({
  type,
  ...notification
}: INotificationOptions): string =>
  notifications.show({
    ...notification,
    color: notificationColors[type],
    "data-notification-type": type,
  });
