import { ICategory } from "./category";

export enum AccountSource {
  Manual = "Manual",
  SimpleFIN = "SimpleFIN",
}

export interface IAccountCreateRequest {
  syncID?: string;
  name: string;
  institutionID?: string;
  type: string;
  subtype: string;
  hideTransactions: boolean;
  hideAccount: boolean;
  source: AccountSource;
}

export interface IAccountUpdateRequest {
  id: string;
  name: string;
  type: string;
  subtype: string;
  hideTransactions: boolean;
  hideAccount: boolean;
  interestRate: number | null;
}

export interface IAccountIndexRequest {
  id: string;
  index: number;
}

export interface IAccountResponse {
  id: string;
  syncID: string;
  name: string;
  institutionID: string;
  type: string;
  subtype: string;
  currentBalance: number;
  balanceDate: Date;
  hideTransactions: boolean;
  hideAccount: boolean;
  deleted: Date | null;
  index: number;
  interestRate: number | null;
  source: string;
  userID: string;
}

export const liabilityAccountTypes = ["Loan", "Mortgage", "Credit Card"];
export const assetAccountTypes = [
  "Checking",
  "Savings",
  "Investment",
  "Cash",
  "Other",
];

export const accountCategories: ICategory[] = [
  {
    value: "Checking",
    parent: "",
  },
  {
    value: "Savings",
    parent: "",
  },
  {
    value: "Money Market",
    parent: "Savings",
  },
  {
    value: "Certificate of Deposit",
    parent: "Savings",
  },
  {
    value: "Loan",
    parent: "",
  },
  {
    value: "Auto",
    parent: "Loan",
  },
  {
    value: "Student",
    parent: "Loan",
  },
  {
    value: "Personal",
    parent: "Loan",
  },
  {
    value: "Home Equity",
    parent: "Loan",
  },
  {
    value: "Credit Card",
    parent: "",
  },
  {
    value: "Investment",
    parent: "",
  },
  {
    value: "401k",
    parent: "Investment",
  },
  {
    value: "Roth IRA",
    parent: "Investment",
  },
  {
    value: "Rollover IRA",
    parent: "Investment",
  },
  {
    value: "ESPP",
    parent: "Investment",
  },
  {
    value: "Trust",
    parent: "Investment",
  },
  {
    value: "Taxable",
    parent: "Investment",
  },
  {
    value: "Mortgage",
    parent: "",
  },
  {
    value: "Cash",
    parent: "",
  },
  {
    value: "Other",
    parent: "",
  },
];
