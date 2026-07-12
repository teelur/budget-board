export interface ITransactionImport {
  date: string | null;
  merchantName: string | null;
  category: string | null;
  amount: number | null;
  account: string | null;
}

export interface ITransactionImportTableData extends ITransactionImport {
  uid: number;
  type: string | null;
}

export interface IAccountNameToIDKeyValuePair {
  accountName: string;
  accountID: string;
}

export interface ITransactionImportRequest {
  transactions: ITransactionImport[];
  accountNameToIDMap: IAccountNameToIDKeyValuePair[];
}

export interface ITransactionCreateRequest {
  syncID: string | null;
  amount: number;
  date: string;
  category: string | null;
  subcategory: string | null;
  merchantName: string | null;
  source: string | null;
  accountID: string;
}

export interface ITransactionUpdateRequest {
  id: string;
  amount?: number;
  date?: string;
  category?: string | null;
  subcategory?: string | null;
  merchantName?: string | null;
}

export interface ITransactionSplitRequest {
  id: string;
  amount: number;
  category: string;
  subcategory: string;
}

export interface ITransaction {
  id: string;
  syncID: string | null;
  amount: number;
  date: string;
  category: string | null;
  subcategory: string | null;
  merchantName: string | null;
  pending: boolean;
  deleted: Date | null;
  source: string;
  accountID: string;
}

export interface IFilters {
  accounts: string[];
  category: string;
  dateRange: [Date | null, Date | null];
  merchantName: string;
  amountRange: [number | null, number | null];
}

export class Filters implements IFilters {
  accounts: string[] = [];
  category: string = "";
  dateRange: [Date | null, Date | null] = [null, null];
  merchantName: string = "";
  amountRange: [number | null, number | null] = [null, null];

  constructor(filter?: Filters) {
    if (filter) {
      this.accounts = filter.accounts;
      this.category = filter.category;
      this.dateRange = filter.dateRange;
      this.merchantName = filter.merchantName;
      this.amountRange = filter.amountRange;
    }
  }

  public isEqual(other: Filters): boolean {
    return (
      JSON.stringify([...this.accounts].sort()) ===
        JSON.stringify([...other.accounts].sort()) &&
      this.category === other.category &&
      this.dateRange[0]?.getTime() === other.dateRange[0]?.getTime() &&
      this.dateRange[1]?.getTime() === other.dateRange[1]?.getTime() &&
      this.merchantName === other.merchantName &&
      this.amountRange[0] === other.amountRange[0] &&
      this.amountRange[1] === other.amountRange[1]
    );
  }
}

export enum TransactionCardType {
  Normal,
  Edit,
  Uncategorized,
}

export const hiddenTransactionCategory = "Hide from Budgets";
export const uncategorizedTransactionCategory = "uncategorized";
