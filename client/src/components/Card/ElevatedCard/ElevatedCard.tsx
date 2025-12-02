import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

export interface ElevatedCardProps extends CardProps {
  hoverEffect?: boolean;
  children?: React.ReactNode;
  ref?: React.Ref<HTMLDivElement>;
  onClick?: (e: React.MouseEvent) => void;
}

const ElevatedCard = ({ children, ...props }: ElevatedCardProps) => {
  return (
    <Card
      ref={props.ref}
      className={elevatedClasses.root}
      onMouseEnter={(e) => {
        if (props.hoverEffect) {
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

export default ElevatedCard;
