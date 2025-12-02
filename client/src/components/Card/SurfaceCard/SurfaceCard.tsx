import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

export interface SurfaceCardProps extends CardProps {
  hoverEffect?: boolean;
  children?: React.ReactNode;
  ref?: React.Ref<HTMLDivElement>;
  onClick?: (e: React.MouseEvent) => void;
}

const SurfaceCard = ({ children, ...props }: SurfaceCardProps) => {
  return (
    <Card
      ref={props.ref}
      className={surfaceClasses.root}
      style={{
        transition: "background 0.2s",
      }}
      onMouseEnter={(e) => {
        if (props.hoverEffect ?? false) {
          e.currentTarget.style.borderColor =
            "var(--mantine-primary-color-filled)";
        }
      }}
      onMouseLeave={(e) => (e.currentTarget.style.borderColor = "")}
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
