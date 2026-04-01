import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { Code, CodeProps } from "@mantine/core";

const BaseCode = ({ ...props }: CodeProps): React.ReactNode => {
  return <Code className={baseClasses.code} {...props} />;
};

export default BaseCode;
