import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { TextInput, TextInputProps } from "@mantine/core";

const BaseTextInput = (props: TextInputProps): React.ReactNode => {
  return <TextInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BaseTextInput;
