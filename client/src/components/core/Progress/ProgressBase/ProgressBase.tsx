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
  type: ProgressType;
  warningThreshold?: number;
}

const ProgressBase = ({
  percentComplete,
  amount,
  limit,
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

  return (
    <Progress.Root {...props} w="100%" radius="xl">
      <Progress.Section value={percentComplete} color={getColor()}>
        <Progress.Label>{percentComplete.toFixed(0)}%</Progress.Label>
      </Progress.Section>
    </Progress.Root>
  );
};

export default ProgressBase;
