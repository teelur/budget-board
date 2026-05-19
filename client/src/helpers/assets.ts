import { IAssetResponse } from "~/models/asset";
import { IAssetType } from "~/models/assetType";
import { areStringsEqual } from "./utils";

export const getIsParentAssetType = (
  assetTypeValue: string,
  assetTypes: IAssetType[],
): boolean => {
  if (assetTypeValue.length === 0) {
    return true;
  }

  return (
    (
      assetTypes.find((t) => areStringsEqual(t.value, assetTypeValue))
        ?.parent ?? ""
    ).length === 0
  );
};

export const getParentAssetType = (
  assetTypeValue: string,
  assetTypes: IAssetType[],
): string => {
  if (assetTypeValue.length === 0) {
    return "";
  }

  const type = assetTypes.find((t) => areStringsEqual(t.value, assetTypeValue));
  if (type == null) {
    return "";
  }

  return type.parent === "" ? type.value : type.parent;
};

/**
 * Filters out assets that are either hidden or marked as deleted.
 * @param assets - An array of asset objects to filter.
 * @returns A filtered array of visible asset objects.
 */
export const filterVisibleAssets = (
  assets: IAssetResponse[],
): IAssetResponse[] =>
  assets.filter((a: IAssetResponse) => !(a.hide || a.deleted !== null));

/**
 * Calculates and returns the total current value from an array of asset objects.
 *
 * @param assets - An array of objects implementing the IAssetResponse interface.
 * @returns The sum of the currentValue properties of all assets. If the assets array is empty, the function returns 0.
 */
export const sumAssetsTotalValue = (assets: IAssetResponse[]) => {
  if (assets.length > 0) {
    return assets.reduce((n, { currentValue }) => n + currentValue, 0);
  }

  return 0;
};

/**
 * Returns assets matching any of the given types.
 * If a given type is a parent type, assets matching any of its subtypes will also be included.
 *
 * @param {IAssetResponse[]} assets - List of asset objects.
 * @param {string[]} typesToGet - List of types or subtypes to match.
 * @param {IAssetType[]} allTypes - List of all asset types, including parent-child relationships.
 * @returns {IAssetResponse[]} Filtered list of matching assets.
 */
export const getAssetsOfTypes = (
  assets: IAssetResponse[],
  typesToGet: string[],
  allTypes: IAssetType[],
): IAssetResponse[] => {
  const typesToGetWithSubtypes = new Set<string>();

  typesToGet.forEach((type) => {
    typesToGetWithSubtypes.add(type);

    // If the type is a parent type, also include its subtypes
    const subtypes = allTypes
      .filter((t) => t.parent === type)
      .map((t) => t.value);
    subtypes.forEach((subtype) => typesToGetWithSubtypes.add(subtype));
  });

  return assets.filter((asset) => typesToGetWithSubtypes.has(asset.type));
};
