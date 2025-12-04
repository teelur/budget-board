import { Progress, ProgressRootProps } from "@mantine/core";
import { getStatusColor, StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";

export interface ProgressBaseProps extends ProgressRootProps {
  percentComplete: number;
  amount: number;
  limit: number;
  isIncome: boolean;
  warningThreshold?: number;
}

const ProgressBase = ({
  percentComplete,
  amount,
  limit,
  isIncome,
  warningThreshold,
  ...props
}: ProgressBaseProps) => {
  return (
    <Progress.Root {...props} w="100%">
      <Progress.Section
        value={percentComplete}
        color={getStatusColor(
          roundAwayFromZero(amount),
          limit,
          isIncome ? StatusColorType.Income : StatusColorType.Expense,
          warningThreshold ?? 80
        )}
      >
        <Progress.Label>{percentComplete.toFixed(0)}%</Progress.Label>
      </Progress.Section>
    </Progress.Root>
  );
};

export default ProgressBase;
