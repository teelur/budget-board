import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { DateInput, DateInputProps } from "@mantine/dates";

const ElevatedDateInput = (props: DateInputProps): React.ReactNode => {
  return <DateInput classNames={{ input: elevatedClasses.input }} {...props} />;
};

export default ElevatedDateInput;
