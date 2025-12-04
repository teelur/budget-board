import React from "react";
import BaseProgress, { BaseProgressProps } from "./BaseProgress/BaseProgress";
import ElevatedProgress, {
  ElevatedProgressProps,
} from "./ElevatedProgress/ElevatedProgress";
import SurfaceProgress, {
  SurfaceProgressProps,
} from "./SurfaceProgress/SurfaceProgress";

export interface ProgressProps
  extends BaseProgressProps,
    SurfaceProgressProps,
    ElevatedProgressProps {
  elevation?: number;
}

const Progress = ({
  elevation = 0,
  ...props
}: ProgressProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseProgress {...props} />;
    case 1:
      return <SurfaceProgress {...props} />;
    case 2:
      return <ElevatedProgress {...props} />;
    default:
      throw new Error("Invalid elevation level for Progress component");
  }
};

export default Progress;
