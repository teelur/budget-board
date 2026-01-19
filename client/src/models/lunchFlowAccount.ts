export interface ILunchFlowAccountResponse {
  id: string;
  name: string;
  syncID: string;
  institutionName: string;
  institutionLogo: string;
  provider: string;
  currency: string;
  status: string;
  balance: number;
  balanceDate: number;
  lastSync?: Date;
  linkedAccountId?: string;
}
