import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { PasswordInput, PasswordInputProps } from "@mantine/core";

const SurfacePasswordInput = (props: PasswordInputProps): React.ReactNode => {
  return (
    <PasswordInput classNames={{ input: surfaceClasses.input }} {...props} />
  );
};

export default SurfacePasswordInput;
