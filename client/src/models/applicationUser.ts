export interface IApplicationUser {
  id: string;
  accessToken: boolean;
  lastSync: Date;
  twoFactorEnabled: boolean;
}

export const defaultGuid: string = "00000000-0000-0000-0000-000000000000";
