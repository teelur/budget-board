import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { NumberInput, NumberInputProps } from "@mantine/core";

const ElevatedNumberInput = (props: NumberInputProps): React.ReactNode => {
  return (
    <NumberInput classNames={{ input: elevatedClasses.input }} {...props} />
  );
};

export default ElevatedNumberInput;
