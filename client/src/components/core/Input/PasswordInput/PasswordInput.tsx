import React from "react";
import { PasswordInputProps as MantinePasswordInputProps } from "@mantine/core";
import BasePasswordInput from "../Base/BasePasswordInput/BasePasswordInput";
import SurfacePasswordInput from "../Surface/SurfacePasswordInput/SurfacePasswordInput";
import ElevatedPasswordInput from "../Elevated/ElevatedPasswordInput/ElevatedPasswordInput";

export interface PasswordInputProps extends MantinePasswordInputProps {
  elevation?: number;
}

const PasswordInput = ({
  elevation = 0,
  ...props
}: PasswordInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BasePasswordInput {...props} />;
    case 1:
      return <SurfacePasswordInput {...props} />;
    case 2:
      return <ElevatedPasswordInput {...props} />;
    default:
      return null;
  }
};

export default PasswordInput;
