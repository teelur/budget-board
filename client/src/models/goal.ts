import { IAccountResponse } from "./account";

export interface IGoalCreateRequest {
  name: string;
  completeDate: Date | null;
  amount: number;
  initialAmount: number | null;
  monthlyContribution: number | null;
  accountIds: string[];
}

export interface IGoalUpdateRequest {
  id: string;
  name: string;
  completeDate: Date | null;
  isCompleteDateEditable: boolean;
  amount: number;
  monthlyContribution: number | null;
  isMonthlyContributionEditable: boolean;
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
