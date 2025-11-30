import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { Card, CardProps } from "@mantine/core";

interface ElevatedCardProps extends CardProps {
  children?: React.ReactNode;
}

const ElevatedCard = ({ children, ...props }: ElevatedCardProps) => {
  return (
    <Card className={elevatedClasses.root} p="0.5rem" withBorder {...props}>
      {children}
    </Card>
  );
};

export default ElevatedCard;
