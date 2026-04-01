import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { TextInput, TextInputProps } from "@mantine/core";

const SurfaceTextInput = (props: TextInputProps): React.ReactNode => {
  return <TextInput classNames={{ input: surfaceClasses.input }} {...props} />;
};

export default SurfaceTextInput;
