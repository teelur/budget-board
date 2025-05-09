import { ICategory } from "./category";

export interface ITransactionImport {
  date: Date | null;
  description: string | null;
  category: string | null;
  amount: number | null;
  account: string | null;
  type: string | null;
}

export interface IAccountNameToIDKeyValuePair {
  accountName: string;
  accountID: string;
}

export interface ITransactionImportRequest {
  transactions: ITransactionCreateRequest[];
  accountNameToIDMap: IAccountNameToIDKeyValuePair[];
}

export interface ITransactionCreateRequest {
  syncID: string | null;
  amount: number;
  date: Date;
  category: string | null;
  subcategory: string | null;
  merchantName: string | null;
  source: string | null;
  accountID: string;
}

export interface ITransactionUpdateRequest {
  id: string;
  amount: number;
  date: Date;
  category: string | null;
  subcategory: string | null;
  merchantName: string | null;
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
  date: Date;
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
}

export class Filters implements IFilters {
  accounts: string[] = [];
  category: string = "";
  dateRange: [Date | null, Date | null] = [null, null];

  constructor(filter?: Filters) {
    if (filter) {
      this.accounts = filter.accounts;
      this.category = filter.category;
      this.dateRange = filter.dateRange;
    }
  }
}

export enum TransactionCardType {
  Normal,
  Edit,
  Uncategorized,
}

export const hiddenTransactionCategory = "Hide from Budgets";

export const defaultTransactionCategories: ICategory[] = [
  {
    value: "Auto & Transport",
    parent: "",
  },
  {
    value: "Auto Insurance",
    parent: "auto & transport",
  },
  {
    value: "Auto Payment",
    parent: "auto & transport",
  },
  {
    value: "Gas & Fuel",
    parent: "auto & transport",
  },
  {
    value: "Parking",
    parent: "auto & transport",
  },
  {
    value: "Public Transportation",
    parent: "auto & transport",
  },
  {
    value: "Ride Share",
    parent: "auto & transport",
  },
  {
    value: "Service & Parts",
    parent: "auto & transport",
  },
  {
    value: "Bills & Utilities",
    parent: "",
  },
  {
    value: "Internet",
    parent: "bills & utilities",
  },
  {
    value: "Mobile Phone",
    parent: "bills & utilities",
  },
  {
    value: "Television",
    parent: "bills & utilities",
  },
  {
    value: "Utilities",
    parent: "bills & utilities",
  },
  {
    value: "Education",
    parent: "",
  },
  {
    value: "Books & Supplies",
    parent: "education",
  },
  {
    value: "Student Loan",
    parent: "education",
  },
  {
    value: "Tuition",
    parent: "education",
  },
  {
    value: "Entertainment",
    parent: "",
  },
  {
    value: "Activities",
    parent: "entertainment",
  },
  {
    value: "Arts",
    parent: "entertainment",
  },
  {
    value: "Movies",
    parent: "entertainment",
  },
  {
    value: "Music",
    parent: "entertainment",
  },
  {
    value: "Books",
    parent: "entertainment",
  },
  {
    value: "Games",
    parent: "entertainment",
  },
  {
    value: "Fees & Charges",
    parent: "",
  },
  {
    value: "ATM Fee",
    parent: "fees & charges",
  },
  {
    value: "Bank Fee",
    parent: "fees & charges",
  },
  {
    value: "Finance Charge",
    parent: "fees & charges",
  },
  {
    value: "Late Fee",
    parent: "fees & charges",
  },
  {
    value: "Service Fee",
    parent: "fees & charges",
  },
  {
    value: "Trade Commissions",
    parent: "fees & charges",
  },
  {
    value: "Financial",
    parent: "",
  },
  {
    value: "Roth IRA",
    parent: "financial",
  },
  {
    value: "Investment",
    parent: "financial",
  },
  {
    value: "Food & Dining",
    parent: "",
  },
  {
    value: "Alcohol & Bars",
    parent: "food & dining",
  },
  {
    value: "Coffee Shops",
    parent: "food & dining",
  },
  {
    value: "Food Delivery",
    parent: "food & dining",
  },
  {
    value: "Groceries",
    parent: "food & dining",
  },
  {
    value: "Restaurants",
    parent: "food & dining",
  },
  {
    value: "Gifts & Donations",
    parent: "",
  },
  {
    value: "Charity",
    parent: "gifts & donations",
  },
  {
    value: "Gift",
    parent: "gifts & donations",
  },
  {
    value: "Goals",
    parent: "",
  },
  {
    value: "Health & Fitness",
    parent: "",
  },
  {
    value: "Dentist",
    parent: "health & fitness",
  },
  {
    value: "Doctor",
    parent: "health & fitness",
  },
  {
    value: "Eyecare",
    parent: "health & fitness",
  },
  {
    value: "Gym",
    parent: "health & fitness",
  },
  {
    value: "Health Insurance",
    parent: "health & fitness",
  },
  {
    value: "Pharmacy",
    parent: "health & fitness",
  },
  {
    value: "Sports",
    parent: "health & fitness",
  },
  {
    // This one is a bit of a special case,
    // since we need to filter this one from certain views
    value: hiddenTransactionCategory,
    parent: "",
  },
  {
    value: "Home",
    parent: "",
  },
  {
    value: "Furnishings",
    parent: "home",
  },
  {
    value: "Home Improvement",
    parent: "home",
  },
  {
    value: "Home Insurance",
    parent: "home",
  },
  {
    value: "Home Services",
    parent: "home",
  },
  {
    value: "Home Supplies",
    parent: "home",
  },
  {
    value: "Lawn & Garden",
    parent: "home",
  },
  {
    value: "Mortgage & Rent",
    parent: "home",
  },
  {
    value: "Income",
    parent: "",
  },
  {
    value: "Bonus",
    parent: "income",
  },
  {
    value: "Interest Income",
    parent: "income",
  },
  {
    value: "Paycheck",
    parent: "income",
  },
  {
    value: "Reimburstments",
    parent: "income",
  },
  {
    value: "Tax Return",
    parent: "income",
  },
  {
    value: "Investments",
    parent: "",
  },
  {
    value: "Buy",
    parent: "investments",
  },
  {
    value: "Deposit",
    parent: "investments",
  },
  {
    value: "Dividend & Cap Gains",
    parent: "investments",
  },
  {
    value: "Sell",
    parent: "investments",
  },
  {
    value: "Withdrawl",
    parent: "investments",
  },
  {
    value: "Loans",
    parent: "",
  },
  {
    value: "Loan Fees & Charges",
    parent: "loans",
  },
  {
    value: "Loan Insurance",
    parent: "loans",
  },
  {
    value: "Loan Interest",
    parent: "loans",
  },
  {
    value: "Loan Payments",
    parent: "loans",
  },
  {
    value: "Loan Principal",
    parent: "loans",
  },
  {
    value: "Misc",
    parent: "",
  },
  {
    value: "Personal Care",
    parent: "",
  },
  {
    value: "Hair",
    parent: "personal care",
  },
  {
    value: "Laundry",
    parent: "personal care",
  },
  {
    value: "Spa & Massage",
    parent: "personal care",
  },
  {
    value: "Pets",
    parent: "",
  },
  {
    value: "Pet Food & Supplies",
    parent: "pets",
  },
  {
    value: "Pet Grooming",
    parent: "pets",
  },
  {
    value: "Veterinary",
    parent: "pets",
  },
  {
    value: "Shopping",
    parent: "",
  },
  {
    value: "Clothing",
    parent: "shopping",
  },
  {
    value: "Electronics & Software",
    parent: "shopping",
  },
  {
    value: "Hobbies",
    parent: "shopping",
  },
  {
    value: "Household",
    parent: "shopping",
  },
  {
    value: "Taxes",
    parent: "",
  },
  {
    value: "Federal Tax",
    parent: "taxes",
  },
  {
    value: "Local Tax",
    parent: "taxes",
  },
  {
    value: "Property Tax",
    parent: "taxes",
  },
  {
    value: "Sales Tax",
    parent: "taxes",
  },
  {
    value: "State Tax",
    parent: "taxes",
  },
  {
    value: "Transfer",
    parent: "",
  },
  {
    value: "Credit Card Payment",
    parent: "transfer",
  },
  {
    value: "Travel",
    parent: "",
  },
  {
    value: "Air Travel",
    parent: "travel",
  },
  {
    value: "Hotel",
    parent: "travel",
  },
  {
    value: "Rental Car & Taxi",
    parent: "travel",
  },
  {
    value: "Vacation",
    parent: "travel",
  },
  {
    value: "Other",
    parent: "",
  },
];
