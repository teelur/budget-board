import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { PinInput, PinInputProps } from "@mantine/core";

const SurfacePinInput = (props: PinInputProps): React.ReactNode => {
  return <PinInput classNames={{ input: surfaceClasses.input }} {...props} />;
};

export default SurfacePinInput;
