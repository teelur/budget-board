import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { Textarea, TextareaProps } from "@mantine/core";

const ElevatedTextarea = (props: TextareaProps): React.ReactNode => {
  return <Textarea classNames={{ input: elevatedClasses.input }} {...props} />;
};

export default ElevatedTextarea;
