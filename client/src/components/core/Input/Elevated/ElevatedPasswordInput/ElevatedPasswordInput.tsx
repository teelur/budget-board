import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { PasswordInput, PasswordInputProps } from "@mantine/core";

const ElevatedPasswordInput = (props: PasswordInputProps): React.ReactNode => {
  return (
    <PasswordInput classNames={{ input: elevatedClasses.input }} {...props} />
  );
};

export default ElevatedPasswordInput;
