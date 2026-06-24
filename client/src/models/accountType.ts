import { ICategory } from "./category";

export interface IAccountType extends ICategory {
  classification: string;
}

export interface IAccountTypeCreateRequest {
  value: string;
  parent: string;
  classification: string;
}

export interface IAccountTypeUpdateRequest {
  id: string;
  value?: string;
  parent?: string;
  classification?: string;
}

export interface IAccountTypeResponse extends IAccountType {
  id: string;
}

export interface IAccountTypeNode extends IAccountType {
  subTypes: IAccountTypeNode[];
}

export class AccountTypeNode implements IAccountTypeNode {
  subTypes: IAccountTypeNode[];
  value: string;
  parent: string;
  classification: string;

  constructor(type?: IAccountType) {
    this.value = type?.value ?? "";
    this.parent = type?.parent ?? "";
    this.classification = type?.classification ?? "";
    this.subTypes = [];
  }
}
