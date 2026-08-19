import elevatedClasses from "~/styles/Elevated.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

export interface ElevatedCardProps extends CardProps {
  hoverEffect?: boolean;
  children?: React.ReactNode;
  ref?: React.Ref<HTMLDivElement>;
  onClick?: (e: React.MouseEvent) => void;
}

const ElevatedCard = ({
  children,
  hoverEffect,
  className,
  ...props
}: ElevatedCardProps) => {
  return (
    <Card
      ref={props.ref}
      className={`${elevatedClasses.root} ${hoverClasses.hoverable}${className ? ` ${className}` : ""}`}
      p={props.p ?? "0.5rem"}
      radius={props.radius ?? "md"}
      shadow={props.shadow ?? "sm"}
      withBorder={props.withBorder ?? true}
      onClick={props.onClick}
      {...props}
      data-hover-effect={hoverEffect ? "true" : undefined}
      data-hover-variant="card"
    >
      {children}
    </Card>
  );
};

export default ElevatedCard;
