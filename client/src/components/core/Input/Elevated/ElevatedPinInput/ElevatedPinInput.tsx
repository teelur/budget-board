import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { PinInput, PinInputProps } from "@mantine/core";

const ElevatedPinInput = (props: PinInputProps): React.ReactNode => {
  return <PinInput classNames={{ input: elevatedClasses.input }} {...props} />;
};

export default ElevatedPinInput;
