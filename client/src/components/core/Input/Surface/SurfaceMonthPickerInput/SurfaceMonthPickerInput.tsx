import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { MonthPickerInput, MonthPickerInputProps } from "@mantine/dates";

const SurfaceMonthPickerInput = (
  props: MonthPickerInputProps,
): React.ReactNode => {
  return (
    <MonthPickerInput classNames={{ input: surfaceClasses.input }} {...props} />
  );
};

export default SurfaceMonthPickerInput;
