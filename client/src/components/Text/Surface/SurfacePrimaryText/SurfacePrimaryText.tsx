import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Text, TextProps } from "@mantine/core";

interface SurfacePrimaryTextProps extends TextProps {
  children: React.ReactNode;
}

const SurfacePrimaryText = ({
  children,
  ...props
}: SurfacePrimaryTextProps): React.ReactNode => (
  <Text className={surfaceClasses.textPrimary} fw={600} {...props}>
    {children}
  </Text>
);

export default SurfacePrimaryText;
