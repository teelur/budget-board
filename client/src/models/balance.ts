export interface IBalanceResponse {
  id: string;
  amount: number;
  dateTime: Date;
  deleted: Date | null;
  accountID: string;
}

export interface IBalanceCreateRequest {
  amount: number;
  dateTime: Date;
  accountID: string;
}

export interface IBalanceUpdateRequest {
  id: string;
  amount: number;
  dateTime: Date;
}
