import { type Account } from '@/types/account';

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
