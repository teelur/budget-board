import React from "react";
import { Text, TextProps } from "@mantine/core";
import { BudgetValueType, getBudgetValueColor } from "~/helpers/budgets";

interface StatusTextProps extends TextProps {
  amount: number;
  total?: number;
  type?: BudgetValueType;
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
      c={getBudgetValueColor(
        amount,
        total ?? 0,
        type ?? BudgetValueType.Total,
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
