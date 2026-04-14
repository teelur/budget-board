export interface IBalanceResponse {
  id: string;
  amount: number;
  date: string;
  accountID: string;
}

export interface IBalanceCreateRequest {
  amount: number;
  date: string;
  accountID: string;
}

export interface IBalanceUpdateRequest {
  id: string;
  amount: number;
  date: string;
}
