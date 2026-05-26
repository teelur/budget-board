import React from "react";
import { Text, TextProps } from "@mantine/core";

interface PrimaryTextProps extends TextProps {
  children?: React.ReactNode;
  elevation?: number;
}

const PrimaryText = ({
  children,
  elevation = 0,
  ...props
}: PrimaryTextProps): React.ReactNode => {
  const getTextColor = (): string => {
    switch (elevation) {
      case 0:
        return "var(--base-color-text-primary)";
      case 1:
        return "var(--surface-color-text-primary)";
      case 2:
        return "var(--elevated-color-text-primary)";
      default:
        return "var(--base-color-text-primary)";
    }
  };

  return (
    <Text c={getTextColor()} fw={500} {...props}>
      {children}
    </Text>
  );
};

export default PrimaryText;
