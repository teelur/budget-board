import { ICategory } from "./category";

export interface IAssetType extends ICategory {}

export interface IAssetTypeCreateRequest extends IAssetType {}

export interface IAssetTypeUpdateRequest extends IAssetType {
  id: string;
}

export interface IAssetTypeResponse extends IAssetType {
  id: string;
}
