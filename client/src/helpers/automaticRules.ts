import dayjs from "dayjs";
import { convertNumberToCurrency } from "./currency";
import { getFormattedCategoryValue } from "./category";
import { ICategory } from "~/models/category";

export const getDefaultValue = (field: string): string => {
  switch (field) {
    case "merchant":
      return "";
    case "amount":
      return "0";
    case "date":
      return dayjs().format("YYYY-MM-DD").toString();
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
  categories: ICategory[]
): string => {
  switch (field) {
    case "merchant":
      return value;
    case "amount":
      return convertNumberToCurrency(parseFloat(value), true, currency);
    case "date":
      return dayjs(value).format("YYYY-MM-DD");
    case "category":
      return getFormattedCategoryValue(value, categories);
    default:
      return value;
  }
};
