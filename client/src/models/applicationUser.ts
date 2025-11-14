export interface IApplicationUser {
  id: string;
  accessToken: boolean;
  lastSync: Date;
  twoFactorEnabled: boolean;
  hasOidcLogin: boolean;
  hasLocalLogin: boolean;
}

export const defaultGuid: string = "00000000-0000-0000-0000-000000000000";
