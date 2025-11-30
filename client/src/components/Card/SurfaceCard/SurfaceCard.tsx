import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

interface SurfaceCardProps extends CardProps {
  children?: React.ReactNode;
  ref?: React.Ref<HTMLDivElement>;
  onClick?: () => void;
}

const SurfaceCard = ({ children, ...props }: SurfaceCardProps) => {
  return (
    <Card
      ref={props.ref}
      className={surfaceClasses.root}
      p={props.p ?? "0.5rem"}
      radius={props.radius ?? "md"}
      shadow={props.shadow ?? "sm"}
      withBorder={props.withBorder ?? true}
      onClick={props.onClick}
      {...props}
    >
      {children}
    </Card>
  );
};

export default SurfaceCard;
