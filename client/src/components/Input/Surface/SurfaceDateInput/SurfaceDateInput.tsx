import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { DateInput, DateInputProps } from "@mantine/dates";

const SurfaceDateInput = (props: DateInputProps): React.ReactNode => {
  return <DateInput classNames={{ input: surfaceClasses.input }} {...props} />;
};

export default SurfaceDateInput;
