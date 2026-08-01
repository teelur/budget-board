import React from "react";
import { Text, TextProps } from "@mantine/core";
import {
  StatusColorType as StatusColorType,
  getStatusColor,
} from "~/helpers/budgets";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface StatusTextProps extends TextProps {
  amount: number;
  total?: number;
  type?: StatusColorType;
  warningThreshold?: number;
  children?: React.ReactNode;
}

const StatusText = ({
  amount,
  total,
  type,
  warningThreshold,
  children,
  ...props
}: StatusTextProps) => {
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const color = isPrivacyModeEnabled
    ? "var(--base-color-text-primary)"
    : getStatusColor(
        amount,
        total ?? 0,
        type ?? StatusColorType.Total,
        warningThreshold ?? 110,
      );

  return (
    <Text
      c={color}
      fw={props.fw ?? 600}
      {...props}
    >
      {children}
    </Text>
  );
};

export default StatusText;
