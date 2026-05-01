export enum AccountSource {
  Manual = "Manual",
  SimpleFIN = "SimpleFIN",
  LunchFlow = "LunchFlow",
}

export interface IAccountCreateRequest {
  name: string;
  institutionID?: string;
  type: string;
  hideTransactions: boolean;
  hideAccount: boolean;
  source: AccountSource;
}

export interface IAccountUpdateRequest {
  id: string;
  name: string;
  type: string;
  hideTransactions: boolean;
  hideAccount: boolean;
  interestRate: number | null;
}

export interface IAccountIndexRequest {
  id: string;
  index: number;
}

export interface IAccountResponse {
  id: string;
  name: string;
  institutionID: string;
  type: string;
  currentBalance: number;
  balanceDate: string | null;
  hideTransactions: boolean;
  hideAccount: boolean;
  deleted: Date | null;
  index: number;
  interestRate: number | null;
  source: string;
  userID: string;
}
