import { IAccountResponse } from "~/models/account";
import { IAccountType } from "~/models/accountType";

/**
 * Filters out accounts that are either hidden or marked as deleted.
 *
 * This function iterates through the provided array of accounts and excludes any account
 * that satisfies either of the following conditions:
 * - The account is marked as hidden (hideAccount is truthy).
 * - The account has a non-null deleted field.
 *
 * @param {IAccountResponse[]} accounts - An array of account objects to filter.
 * @returns {IAccountResponse[]} An array containing only the visible accounts.
 */
export const filterVisibleAccounts = (
  accounts: IAccountResponse[],
): IAccountResponse[] =>
  accounts.filter(
    (a: IAccountResponse) => !(a.hideAccount || a.deleted !== null),
  );

/**
 * Calculates and returns the total current balance from an array of account objects.
 *
 * @param accounts - An array of objects implementing the IAccount interface.
 * @returns The sum of the currentBalance properties of all accounts. If the accounts array is empty, the function returns 0.
 */
export const sumAccountsTotalBalance = (accounts: IAccountResponse[]) => {
  if (accounts.length > 0) {
    return accounts.reduce((n, { currentBalance }) => n + currentBalance, 0);
  }

  return 0;
};

/**
 * Returns accounts matching any of the given types.
 * If a given type is a parent type, accounts matching any of its subtypes will also be included.
 *
 * @param {IAccountResponse[]} accounts - List of account objects.
 * @param {string[]} typesToGet - List of types or subtypes to match.
 * @param {IAccountType[]} allTypes - List of all account types, including parent-child relationships.
 * @returns {IAccountResponse[]} Filtered list of matching accounts.
 */
export const getAccountsOfTypes = (
  accounts: IAccountResponse[],
  typesToGet: string[],
  allTypes: IAccountType[],
): IAccountResponse[] => {
  const typesToGetWithSubtypes = new Set<string>();

  typesToGet.forEach((type) => {
    typesToGetWithSubtypes.add(type);

    // If the type is a parent type, also include its subtypes
    const subtypes = allTypes
      .filter((t) => t.parent === type)
      .map((t) => t.value);
    subtypes.forEach((subtype) => typesToGetWithSubtypes.add(subtype));
  });

  return accounts.filter((account) => typesToGetWithSubtypes.has(account.type));
};
