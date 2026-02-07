export interface ISimpleFinAccountResponse {
  id: string;
  syncID: string;
  name: string;
  currency: string;
  balance: number;
  balanceDate: Date;
  lastSync: Date;
  organizationId?: string;
  linkedAccountId?: string;
}
