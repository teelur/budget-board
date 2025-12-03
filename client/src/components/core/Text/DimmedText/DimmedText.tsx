import React from "react";
import { Text, TextProps } from "@mantine/core";

interface DimmedTextProps extends TextProps {
  children: React.ReactNode;
}

const DimmedText = ({
  children,
  ...props
}: DimmedTextProps): React.ReactNode => (
  <Text c="var(--base-color-text-dimmed)" fw={600} {...props}>
    {children}
  </Text>
);

export default DimmedText;
