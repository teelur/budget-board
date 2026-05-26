import React from "react";
import { Title, TitleProps } from "@mantine/core";

interface SecondaryHeadingProps extends TitleProps {
  children?: React.ReactNode;
}

const SecondaryHeading = ({
  children,
  ...props
}: SecondaryHeadingProps): React.ReactNode => (
  <Title c="var(--base-color-text-secondary)" {...props}>
    {children}
  </Title>
);

export default SecondaryHeading;
