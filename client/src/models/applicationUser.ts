export interface IApplicationUserResponse {
  id: string;
  simpleFinAccessToken: boolean;
  lunchFlowApiKey: boolean;
  lastSync: Date;
  twoFactorEnabled: boolean;
  hasOidcLogin: boolean;
  hasLocalLogin: boolean;
  isSuperuser: boolean;
}

export const defaultGuid: string = "00000000-0000-0000-0000-000000000000";
