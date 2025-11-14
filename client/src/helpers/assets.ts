import { IAssetResponse } from "~/models/asset";

/**
 * Filters out assets that are either hidden or marked as deleted.
 * @param assets - An array of asset objects to filter.
 * @returns A filtered array of visible asset objects.
 */
export const filterVisibleAssets = (
  assets: IAssetResponse[]
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
