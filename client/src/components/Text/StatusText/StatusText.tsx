import React from "react";
import { Text, TextProps } from "@mantine/core";

interface StatusTextProps extends TextProps {
  value: number;
  children: React.ReactNode;
}

const StatusText = ({ value, children, ...props }: StatusTextProps) => (
  <Text
    c={
      value < 0
        ? "var(--text-color-status-bad)"
        : "var(--text-color-status-good)"
    }
    fw={600}
    {...props}
  >
    {children}
  </Text>
);

export default StatusText;
