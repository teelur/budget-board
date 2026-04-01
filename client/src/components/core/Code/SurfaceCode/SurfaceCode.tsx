import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Code, CodeProps } from "@mantine/core";

const SurfaceCode = ({ ...props }: CodeProps): React.ReactNode => {
  return <Code className={surfaceClasses.code} {...props} />;
};

export default SurfaceCode;
