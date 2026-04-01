import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { PinInput, PinInputProps } from "@mantine/core";

const BasePinInput = (props: PinInputProps): React.ReactNode => {
  return <PinInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BasePinInput;
