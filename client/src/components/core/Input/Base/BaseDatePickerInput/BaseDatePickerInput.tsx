import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { DatePickerInput, DatePickerInputProps } from "@mantine/dates";

const BaseDatePickerInput = (
  props: DatePickerInputProps<"range">
): React.ReactNode => {
  return (
    <DatePickerInput classNames={{ input: baseClasses.input }} {...props} />
  );
};

export default BaseDatePickerInput;
