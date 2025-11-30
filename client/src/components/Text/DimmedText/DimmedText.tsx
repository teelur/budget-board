import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Text, TextProps } from "@mantine/core";

interface DimmedTextProps extends TextProps {
  children: React.ReactNode;
}

const DimmedText = ({
  children,
  ...props
}: DimmedTextProps): React.ReactNode => (
  <Text className={surfaceClasses.textDimmed} fw={600} {...props}>
    {children}
  </Text>
);

export default DimmedText;
