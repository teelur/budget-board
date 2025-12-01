import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

export interface BaseCardProps extends CardProps {
  hoverEffect?: boolean;
  children?: React.ReactNode;
  ref?: React.Ref<HTMLDivElement>;
  onClick?: () => void;
}

const BaseCard = ({ children, ...props }: BaseCardProps) => {
  return (
    <Card
      ref={props.ref}
      className={baseClasses.root}
      style={{
        transition: "background 0.2s",
      }}
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

export default BaseCard;
