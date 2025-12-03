import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { TextInput, TextInputProps } from "@mantine/core";

const ElevatedTextInput = (props: TextInputProps): React.ReactNode => {
  return <TextInput classNames={{ input: elevatedClasses.input }} {...props} />;
};

export default ElevatedTextInput;
