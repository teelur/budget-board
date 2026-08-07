export type TwoFactorAuthResponse = {
  sharedKey: string;
  recoveryCodesLeft: number;
  recoveryCodes: string[] | null;
  isTwoFactorEnabled: boolean;
  isMachineRemembered: boolean;
};

export type TwoFactorAuthRequest = {
  enable?: boolean;
  twoFactorCode?: string;
  resetSharedKey: boolean;
  resetRecoveryCodes: boolean;
  forgetMachine: boolean;
};
