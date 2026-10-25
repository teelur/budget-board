import React from "react";
import classes from "./LoadingScreen.module.css";

const LoadingScreen = (): React.ReactNode => {
  return (
    <div
      className={`${classes.loadingContainer} ${classes.loadingContainerFullscreen}`}
    >
      <div className={classes.loadingSpinner} />
    </div>
  );
};

export default LoadingScreen;
