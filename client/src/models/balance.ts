export interface IBalanceResponse {
  id: string;
  amount: number;
  dateTime: string;
  deleted: Date | null;
  accountID: string;
}

export interface IBalanceCreateRequest {
  amount: number;
  dateTime: string;
  accountID: string;
}

export interface IBalanceUpdateRequest {
  id: string;
  amount: number;
  dateTime: string;
}
