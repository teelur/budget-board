import { ISimpleFinAccountResponse } from "./simpleFinAccount";

export interface ISimpleFinOrganizationResponse {
  id: string;
  domain?: string;
  simpleFinUrl: string;
  name?: string;
  url?: string;
  syncID?: string;
  accounts: ISimpleFinAccountResponse[];
}
