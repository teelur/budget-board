export interface IAssetCreateRequest {
  name: string;
}

export interface IAssetUpdateRequest {
  id: string;
  name: string;
  purchasedDate: Date | null;
  purchasePrice: number | null;
  soldDate: Date | null;
  soldPrice: number | null;
  hideProperty: boolean;
}

export interface IAssetIndexRequest {
  id: string;
  index: number;
}

export interface IAssetResponse {
  id: string;
  name: string;
  currentValue: number;
  valueDate: Date;
  purchasedDate: Date | null;
  purchasePrice: number | null;
  soldDate: Date | null;
  soldPrice: number | null;
  hideProperty: boolean;
  deleted: boolean;
  index: number;
  userID: string;
}
