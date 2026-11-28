import React from "react";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";
import NotificationDestinationEditForm from "./NotificationDestinationEditForm/NotificationDestinationEditForm";
import NotificationDestinationFields from "../NotificationDestinationFields/NotificationDestinationFields";

interface NotificationDestinationFormProps {
  destination?: INotificationDestinationResponse;
  onClose: () => void;
}

const NotificationDestinationForm = ({
  destination,
  onClose,
}: NotificationDestinationFormProps): React.ReactNode => {
  if (destination) {
    return (
      <NotificationDestinationEditForm
        destination={destination}
        onClose={onClose}
      />
    );
  }

  return <NotificationDestinationFields onClose={onClose} />;
};

export default NotificationDestinationForm;
