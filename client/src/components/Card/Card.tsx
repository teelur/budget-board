import { CardProps as MantineCardProps } from "@mantine/core";
import React from "react";
import SurfaceCard from "./SurfaceCard/SurfaceCard";
import ElevatedCard from "./ElevatedCard/ElevatedCard";

export interface CardProps extends MantineCardProps {
  children?: React.ReactNode;
  elevation?: number;
  onClick?: () => void;
}

const Card = ({ children, elevation = 0, onClick, ...props }: CardProps) => {
  switch (elevation) {
    case 0:
    case 1:
      return (
        <SurfaceCard {...props} onClick={onClick}>
          {children}
        </SurfaceCard>
      );
    case 2:
      return (
        <ElevatedCard {...props} onClick={onClick}>
          {children}
        </ElevatedCard>
      );
    default:
      return (
        <SurfaceCard {...props} onClick={onClick}>
          {children}
        </SurfaceCard>
      );
  }
};

export default Card;
