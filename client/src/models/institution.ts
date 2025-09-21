import { IAccountResponse } from "./account";

export interface IInstitution {
  id: string;
  name: string;
  index: number;
  deleted: Date | null;
  userID: string;
  accounts: IAccountResponse[];
}

export interface InstitutionIndexRequest {
  id: string;
  index: number;
}

export interface IInstitutionCreateRequest {
  name: string;
}

export interface IInstitutionUpdateRequest {
  id: string;
  name: string;
}
