import type { AxiosResponse } from 'axios';
import request from './request';
import { type Account } from '@/types/account';

export const getAccounts = async (): Promise<AxiosResponse> =>
  await request({
    url: '/api/account',
    method: 'GET',
  });

export const getAccount = async (guid: string): Promise<AxiosResponse> => {
  return await request({
    url: '/api/account',
    method: 'GET',
    params: { guid },
  });
};

export const filterVisibleAccounts = (accounts: Account[]): Account[] => {
  return accounts.filter((a: Account) => !(a.hideAccount || a.deleted !== null));
};

export const updateAccount = async (newAccount: Account): Promise<AxiosResponse> => {
  return await request({
    url: '/api/account',
    method: 'PUT',
    data: newAccount,
  });
};

export const deleteAccount = async (
  guid: string,
  deleteTransactions: boolean
): Promise<AxiosResponse> => {
  return await request({
    url: '/api/account',
    method: 'DELETE',
    params: { guid, deleteTransactions },
  });
};

export const restoreAccount = async (guid: string): Promise<AxiosResponse> => {
  return await request({
    url: '/api/account/restore',
    method: 'POST',
    params: { guid },
  });
};

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
