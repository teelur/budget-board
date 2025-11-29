import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Text, TextProps } from "@mantine/core";

interface SurfaceDimmedTextProps extends TextProps {
  children: React.ReactNode;
}

const SurfaceDimmedText = ({
  children,
  ...props
}: SurfaceDimmedTextProps): React.ReactNode => (
  <Text className={surfaceClasses.textDimmed} fw={600} {...props}>
    {children}
  </Text>
);

export default SurfaceDimmedText;
