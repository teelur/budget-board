import React from "react";
import { Text, TextProps } from "@mantine/core";
import {
  StatusColorType as StatusColorType,
  getStatusColor,
} from "~/helpers/budgets";

interface StatusTextProps extends TextProps {
  amount: number;
  total?: number;
  type?: StatusColorType;
  warningThreshold?: number;
  children: React.ReactNode;
}

const StatusText = ({
  amount,
  total,
  type,
  warningThreshold,
  children,
  ...props
}: StatusTextProps) => {
  return (
    <Text
      c={getStatusColor(
        amount,
        total ?? 0,
        type ?? StatusColorType.Total,
        warningThreshold ?? 110
      )}
      fw={props.fw ?? 600}
      {...props}
    >
      {children}
    </Text>
  );
};

export default StatusText;
