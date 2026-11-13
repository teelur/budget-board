export interface ICreateManagedUserRequest {
  username: string;
  email: string | null;
}

export interface IUpdateManagedUserUsernameRequest {
  username: string;
}

export interface IManagedUserResponse {
  id: string;
  username: string;
  email: string | null;
  hasPassword: boolean;
  hasOidcLogin: boolean;
  isDisabled: boolean;
  mustChangePassword: boolean;
  isSuperuser: boolean;
}

export interface IManagedUserOperationResponse {
  user: IManagedUserResponse;
  temporaryPassword: string | null;
}
