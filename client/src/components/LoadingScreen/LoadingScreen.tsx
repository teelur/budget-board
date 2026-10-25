import React from "react";
import classes from "./LoadingScreen.module.css";

interface LoadingScreenProps {
  message?: string;
}

const LoadingScreen = ({ message }: LoadingScreenProps): React.ReactNode => {
  return (
    <div
      className={`${classes.loadingContainer} ${classes.loadingContainerFullscreen}`}
      role={message ? "status" : undefined}
      aria-live={message ? "polite" : undefined}
    >
      <div className={classes.loadingSpinner} aria-hidden="true" />
      {message && <span className={classes.loadingMessage}>{message}</span>}
    </div>
  );
};

export default LoadingScreen;
