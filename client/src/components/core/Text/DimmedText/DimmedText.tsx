import React from "react";
import { Text, TextProps } from "@mantine/core";

interface DimmedTextProps extends TextProps {
  children?: React.ReactNode;
  elevation?: number;
}

const DimmedText = ({
  children,
  elevation = 0,
  ...props
}: DimmedTextProps): React.ReactNode => {
  const getTextColor = (): string => {
    switch (elevation) {
      case 0:
        return "var(--base-color-text-dimmed)";
      case 1:
        return "var(--surface-color-text-dimmed)";
      case 2:
        return "var(--elevated-color-text-dimmed)";
      default:
        return "var(--base-color-text-dimmed)";
    }
  };

  return (
    <Text c={getTextColor()} fw={500} {...props}>
      {children}
    </Text>
  );
};

export default DimmedText;
