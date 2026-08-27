import { Progress, ProgressRootProps } from "@mantine/core";
import { getStatusColor, StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";

export enum ProgressType {
  Default,
  Income,
  Expense,
}

export interface ProgressBaseProps extends ProgressRootProps {
  percentComplete: number;
  amount: number;
  limit: number;
  projectedAmount?: number;
  type: ProgressType;
  warningThreshold?: number;
}

const ProgressBase = ({
  percentComplete,
  amount,
  limit,
  projectedAmount,
  type,
  warningThreshold,
  ...props
}: ProgressBaseProps) => {
  const getColor = (): string | undefined => {
    switch (type) {
      case ProgressType.Income:
        return getStatusColor(
          roundAwayFromZero(amount),
          limit,
          StatusColorType.Income,
          warningThreshold ?? 80
        );
      case ProgressType.Expense:
        return getStatusColor(
          roundAwayFromZero(amount),
          limit,
          StatusColorType.Expense,
          warningThreshold ?? 80
        );
      default:
        return undefined;
    }
  };

  const direction = type === ProgressType.Expense ? -1 : 1;
  const actualValue = Math.min(100, Math.max(0, percentComplete));
  const projectedPercentComplete =
    projectedAmount === undefined || limit <= 0
      ? actualValue
      : Math.min(
          100,
          Math.max(0, ((projectedAmount * direction) / limit) * 100),
        );
  const projectedValue = Math.max(
    0,
    projectedPercentComplete - actualValue,
  );

  return (
    <Progress.Root {...props} w="100%" radius="xl">
      <Progress.Section value={actualValue} color={getColor()}>
        <Progress.Label>{percentComplete.toFixed(0)}%</Progress.Label>
      </Progress.Section>
      {projectedValue > 0 && (
        <Progress.Section
          value={projectedValue}
          aria-label="Projected recurring transactions"
          style={{
            background:
              "repeating-linear-gradient(135deg, var(--mantine-color-gray-5) 0 3px, transparent 3px 6px)",
          }}
        />
      )}
    </Progress.Root>
  );
};

export default ProgressBase;
