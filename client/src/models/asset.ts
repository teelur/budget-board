export interface IAssetCreateRequest {
  name: string;
}

export interface IAssetUpdateRequest {
  id: string;
  name: string;
  purchaseDate: string | null;
  purchasePrice: number | null;
  sellDate: string | null;
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
  valueDate: string | null;
  purchaseDate: string | null;
  purchasePrice: number | null;
  sellDate: string | null;
  sellPrice: number | null;
  hide: boolean;
  deleted: boolean;
  index: number;
  userID: string;
}
