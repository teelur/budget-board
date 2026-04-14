export interface IValueCreateRequest {
  amount: number;
  date: string;
  assetID: string;
}

export interface IValueUpdateRequest {
  id: string;
  amount: number;
  date: string;
}

export interface IValueResponse {
  id: string;
  amount: number;
  date: string;
  assetID: string;
}
