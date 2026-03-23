export interface IApplicationUser {
  id: string;
  simpleFinAccessToken: boolean;
  lunchFlowApiKey: boolean;
  toshlAccessToken: boolean;
  toshlAccessTokenSuffix: string;
  toshlAccessTokenLength: number;
  toshlLastSync: Date;
  lastSync: Date;
  twoFactorEnabled: boolean;
  hasOidcLogin: boolean;
  hasLocalLogin: boolean;
}

export const defaultGuid: string = "00000000-0000-0000-0000-000000000000";
