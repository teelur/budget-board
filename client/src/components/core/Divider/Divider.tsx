import {
  Divider as MantineDivider,
  DividerProps as MantineDividerProps,
} from "@mantine/core";
import React from "react";

export interface DividerProps extends MantineDividerProps {
  elevation?: number;
}

const Divider = ({ elevation, ...props }: DividerProps): React.ReactNode => {
  const getBorderColor = (): string => {
    switch (elevation) {
      case 0:
        return "var(--base-color-border)";
      case 1:
        return "var(--surface-color-border)";
      case 2:
        return "var(--elevated-color-border)";
      default:
        return "var(--base-color-border)";
    }
  };

  return <MantineDivider color={getBorderColor()} {...props} />;
};

export default Divider;
