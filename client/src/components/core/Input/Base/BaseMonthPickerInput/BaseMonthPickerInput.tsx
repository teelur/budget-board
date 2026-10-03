import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { MonthPickerInput, MonthPickerInputProps } from "@mantine/dates";

const BaseMonthPickerInput = (
  props: MonthPickerInputProps,
): React.ReactNode => {
  return (
    <MonthPickerInput classNames={{ input: baseClasses.input }} {...props} />
  );
};

export default BaseMonthPickerInput;
