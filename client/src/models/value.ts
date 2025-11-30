export interface IValueCreateRequest {
  amount: number;
  dateTime: Date;
  assetID: string;
}

export interface IValueUpdateRequest {
  id: string;
  amount: number;
  dateTime: Date;
}

export interface IValueResponse {
  id: string;
  amount: number;
  dateTime: Date;
  deleted: Date | null;
  assetID: string;
}
