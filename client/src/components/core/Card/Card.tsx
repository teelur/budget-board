import React from "react";
import SurfaceCard, { SurfaceCardProps } from "./SurfaceCard/SurfaceCard";
import ElevatedCard, { ElevatedCardProps } from "./ElevatedCard/ElevatedCard";
import BaseCard, { BaseCardProps } from "./BaseCard/BaseCard";

export interface CardProps
  extends BaseCardProps,
    SurfaceCardProps,
    ElevatedCardProps {
  elevation?: number;
  hoverEffect?: boolean;
}

const Card = ({
  elevation = 0,
  onClick,
  children,
  ...props
}: CardProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return (
        <BaseCard {...props} onClick={onClick}>
          {children}
        </BaseCard>
      );
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
      return null;
  }
};

export default Card;
