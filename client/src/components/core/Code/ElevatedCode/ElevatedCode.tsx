import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { Code, CodeProps } from "@mantine/core";

const ElevatedCode = ({ ...props }: CodeProps): React.ReactNode => {
  return <Code className={elevatedClasses.code} {...props} />;
};

export default ElevatedCode;
