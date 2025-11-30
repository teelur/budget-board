import textClasses from "~/styles/Text.module.css";

import React from "react";
import { Text, TextProps } from "@mantine/core";

interface PrimaryTextProps extends TextProps {
  children: React.ReactNode;
}

const PrimaryText = ({
  children,
  ...props
}: PrimaryTextProps): React.ReactNode => (
  <Text className={textClasses.primary} fw={600} {...props}>
    {children}
  </Text>
);

export default PrimaryText;
