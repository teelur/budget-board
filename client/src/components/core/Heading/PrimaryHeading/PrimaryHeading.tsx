import React from "react";
import { Title, TitleProps } from "@mantine/core";

interface PrimaryHeadingProps extends TitleProps {
  children?: React.ReactNode;
}

const PrimaryHeading = ({
  children,
  ...props
}: PrimaryHeadingProps): React.ReactNode => (
  <Title c="var(--base-color-text-primary)" {...props}>
    {children}
  </Title>
);

export default PrimaryHeading;
