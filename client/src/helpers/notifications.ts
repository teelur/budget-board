import { notifications, type NotificationData } from "@mantine/notifications";

export enum NotificationType {
  Error = "error",
  Success = "success",
  Warning = "warning",
  Information = "information",
}

const notificationColors: Record<NotificationType, string> = {
  [NotificationType.Error]: "var(--button-color-destructive)",
  [NotificationType.Success]: "var(--button-color-confirm)",
  [NotificationType.Warning]: "var(--button-color-warning)",
  [NotificationType.Information]: "var(--text-color-status-neutral)",
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
