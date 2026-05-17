import React from "react";
import classes from "./LoadingScreen.module.css";

interface LoadingScreenProps {
  fullScreen?: boolean;
}

const LoadingScreen = ({
  fullScreen = true,
}: LoadingScreenProps): React.ReactNode => {
  return (
    <div
      className={`${classes.loadingContainer} ${fullScreen ? classes.loadingContainerFullscreen : classes.loadingContainerContained}`}
    >
      <div className={classes.loadingSpinner} />
    </div>
  );
};

export default LoadingScreen;
