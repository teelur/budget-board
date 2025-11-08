export interface IValueCreateRequest {
  amount: number;
  dateTime: Date;
  assetID: string;
}

export interface IValueResponse {
  id: string;
  amount: number;
  dateTime: Date;
  assetID: string;
}
