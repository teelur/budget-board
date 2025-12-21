import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { DatePickerInput, DatePickerInputProps } from "@mantine/dates";

const ElevatedDatePickerInput = (
  props: DatePickerInputProps<"range">
): React.ReactNode => {
  return (
    <DatePickerInput classNames={{ input: elevatedClasses.input }} {...props} />
  );
};

export default ElevatedDatePickerInput;
