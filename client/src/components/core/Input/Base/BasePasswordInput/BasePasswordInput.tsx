import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { PasswordInput, PasswordInputProps } from "@mantine/core";

const BasePasswordInput = (props: PasswordInputProps): React.ReactNode => {
  return <PasswordInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BasePasswordInput;
