import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { Textarea, TextareaProps } from "@mantine/core";

const BaseTextarea = (props: TextareaProps): React.ReactNode => {
  return <Textarea classNames={{ input: baseClasses.input }} {...props} />;
};

export default BaseTextarea;
