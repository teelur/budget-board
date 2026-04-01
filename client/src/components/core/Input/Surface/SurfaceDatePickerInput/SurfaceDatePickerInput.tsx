import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { DatePickerInput, DatePickerInputProps } from "@mantine/dates";

const SurfaceDatePickerInput = (
  props: DatePickerInputProps<"range">
): React.ReactNode => {
  return (
    <DatePickerInput classNames={{ input: surfaceClasses.input }} {...props} />
  );
};

export default SurfaceDatePickerInput;
