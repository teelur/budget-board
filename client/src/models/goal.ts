import { IAccountResponse } from "./account";

export interface IGoalCreateRequest {
  name: string;
  completeDate: string | null;
  amount: number;
  applyExistingBalanceTowardsGoal: boolean;
  monthlyContribution: number | null;
  accountIds: string[];
}

export interface IGoalUpdateRequest {
  id: string;
  name?: string;
  completeDate?: string | null;
  amount?: number;
  monthlyContribution?: number | null;
}

export interface IGoalResponse {
  id: string;
  name: string;
  completeDate: Date;
  isCompleteDateEditable: boolean;
  amount: number;
  initialAmount: number;
  monthlyContribution: number;
  isMonthlyContributionEditable: boolean;
  monthlyContributionProgress: number;
  interestRate: number | null;
  completed: Date | null;
  percentComplete: number;
  accounts: IAccountResponse[];
  userID: string;
}

export enum GoalType {
  None = "",
  SaveGoal = "saveGoal",
  PayGoal = "payGoal",
}

export enum GoalCondition {
  TimedGoal = "timedGoal",
  MonthlyGoal = "monthlyGoal",
}

export enum GoalTarget {
  TargetBalanceGoal = "targetBalanceGoal",
  TargetAmountGoal = "targetAmountGoal",
}
