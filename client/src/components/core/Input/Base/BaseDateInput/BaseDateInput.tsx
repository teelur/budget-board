import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { DateInput, DateInputProps } from "@mantine/dates";

const BaseDateInput = (props: DateInputProps): React.ReactNode => {
  return <DateInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BaseDateInput;
