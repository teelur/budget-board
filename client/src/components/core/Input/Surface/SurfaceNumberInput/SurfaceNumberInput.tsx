import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { NumberInput, NumberInputProps } from "@mantine/core";

const SurfaceNumberInput = (props: NumberInputProps): React.ReactNode => {
  return (
    <NumberInput classNames={{ input: surfaceClasses.input }} {...props} />
  );
};

export default SurfaceNumberInput;
