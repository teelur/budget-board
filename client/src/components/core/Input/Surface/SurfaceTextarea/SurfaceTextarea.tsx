import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Textarea, TextareaProps } from "@mantine/core";

const SurfaceTextarea = (props: TextareaProps): React.ReactNode => {
  return <Textarea classNames={{ input: surfaceClasses.input }} {...props} />;
};

export default SurfaceTextarea;
