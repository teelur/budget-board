export interface IAssetCreateRequest {
  name: string;
}

export interface IAssetUpdateRequest {
  id: string;
  name: string;
  purchaseDate: Date | null;
  purchasePrice: number | null;
  sellDate: Date | null;
  sellPrice: number | null;
  hide: boolean;
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
  purchaseDate: Date | null;
  purchasePrice: number | null;
  sellDate: Date | null;
  sellPrice: number | null;
  hide: boolean;
  deleted: boolean;
  index: number;
  userID: string;
}
