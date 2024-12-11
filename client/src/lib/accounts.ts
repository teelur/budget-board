import { type Account } from '@/types/account';

/**
 * Creates a map of accounts to their institutions.
 * @param accounts Acounts you wish to group by institution.
 * @returns A map of accounts to institutions.
 */
export const groupAccountsByInstitution = (accounts: Account[]): Map<string, Account[]> =>
  accounts.reduce(
    (accountMap: Map<string, Account[]>, item: Account) =>
      accountMap.set(item.institution, [
        ...(accountMap.get(item.institution) || []),
        item,
      ]),
    new Map<string, Account[]>()
  );

export const filterVisibleAccounts = (accounts: Account[]): Account[] =>
  accounts.filter((a: Account) => !(a.hideAccount || a.deleted !== null));

export const getAccountsById = (accountIds: string[], accounts: Account[]): Account[] => {
  let selectedAccounts: Account[] = [];
  accountIds.forEach((accountId) => {
    const foundAccount = accounts.find((account) => account.id === accountId);
    if (foundAccount) selectedAccounts.push(foundAccount);
  });

  return selectedAccounts;
};

export const sumAccountsTotalBalance = (accounts: Account[]) => {
  if (accounts.length > 0) {
    return accounts.reduce((n, { currentBalance }) => n + currentBalance, 0);
  } else {
    return 0;
  }
};
