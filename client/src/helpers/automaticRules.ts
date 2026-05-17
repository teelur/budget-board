import dayjs from "dayjs";
import { convertNumberToCurrency, SignDisplay } from "./currency";
import { getFormattedCategoryValue } from "./category";
import { ICategory } from "~/models/category";
import { IAccountResponse } from "~/models/account";

export const getDefaultValue = (field: string): string => {
  switch (field) {
    case "merchant":
      return "";
    case "amount":
      return "0";
    case "date":
      return dayjs().format("YYYY-MM-DD");
    case "category":
      return "";
    default:
      return "";
  }
};

export const getFormattedValue = (
  field: string,
  value: string,
  currency: string,
  categories: ICategory[],
  formatDate: (dateStr: string) => string,
  intlLocale: string,
  accounts: IAccountResponse[] = [],
): string => {
  switch (field) {
    case "merchant":
      return value;
    case "amount":
      return convertNumberToCurrency(
        parseFloat(value),
        true,
        currency,
        SignDisplay.Auto,
        intlLocale,
      );
    case "date":
      return formatDate(value);
    case "category":
      return getFormattedCategoryValue(value, categories);
    case "account": {
      const accountNames = accounts
        .filter((account) => value.split(",").includes(account.id))
        .map((account) => account.name);
      return accountNames.join(", ");
    }
    default:
      return value;
  }
};
