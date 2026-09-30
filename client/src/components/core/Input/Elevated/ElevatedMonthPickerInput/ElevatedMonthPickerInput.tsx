import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { MonthPickerInput, MonthPickerInputProps } from "@mantine/dates";

const ElevatedMonthPickerInput = (
  props: MonthPickerInputProps,
): React.ReactNode => {
  return (
    <MonthPickerInput
      classNames={{ input: elevatedClasses.input }}
      {...props}
    />
  );
};

export default ElevatedMonthPickerInput;
