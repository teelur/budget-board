import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { NumberInput, NumberInputProps } from "@mantine/core";

const BaseNumberInput = (props: NumberInputProps): React.ReactNode => {
  return <NumberInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BaseNumberInput;
