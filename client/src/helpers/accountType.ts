import { IAccountType } from "~/models/accountType";
import { areStringsEqual } from "./utils";

/**
 * Checks if the specified account type is a root (parent) type.
 *
 * @param {string} accountTypeValue - The account type value to look for.
 * @param {IAccountType[]} accountTypes - An array of all available account types.
 * @returns {boolean} True if the account type is a parent type, false otherwise.
 */
export const getIsParentAccountType = (
  accountTypeValue: string,
  accountTypes: IAccountType[],
): boolean => {
  if (accountTypeValue.length === 0) {
    return true;
  }

  return (
    (
      accountTypes.find((c) => areStringsEqual(c.value, accountTypeValue))
        ?.parent ?? ""
    ).length === 0
  );
};

/**
 * Retrieves the account type's parent value, or returns the account type's own value if it has no parent.
 * If the account type cannot be found, an empty string is returned.
 *
 * @param accountTypeValue - The account type value to locate.
 * @param accountTypes - The list of available account types.
 * @returns The parent account type's value, the account type's own value if it is a root, or an empty string if not found.
 */
export const getParentAccountType = (
  accountTypeValue: string,
  accountTypes: IAccountType[],
): string => {
  if (accountTypeValue.length === 0) {
    return "";
  }

  const type = accountTypes.find((c) =>
    areStringsEqual(c.value, accountTypeValue),
  );
  if (type == null) {
    return "";
  }

  return type.parent === "" ? type.value : type.parent;
};
