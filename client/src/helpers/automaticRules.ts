import dayjs from "dayjs";
import { convertNumberToCurrency, SignDisplay } from "./currency";
import { getFormattedCategoryValue } from "./category";
import { ICategory } from "~/models/category";
import { IAccountResponse } from "~/models/account";
import { getTrimmedUniqueTags } from "~/helpers/tags";
import {
  ActionTransactionFields,
  getActionOperators,
  IRuleParameterEdit,
} from "~/models/automaticRule";
import { isNumericLiteral } from "./automaticRuleExpressions";

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
    case "note":
      return "";
    case "tags":
      return "[]";
    default:
      return "";
  }
};

export const getDefaultAction = (): IRuleParameterEdit => {
  const field =
    ActionTransactionFields.find((item) => item.value === "merchant")?.value ??
    "merchant";

  return {
    field,
    operator: getActionOperators(field)[0]?.value ?? "set",
    value: getDefaultValue(field),
  };
};

export const serializeActionTags = (tags: string[]): string =>
  JSON.stringify(getTrimmedUniqueTags(tags));

export const deserializeActionTags = (value: string): string[] => {
  try {
    const parsed: unknown = JSON.parse(value);
    return Array.isArray(parsed)
      ? getTrimmedUniqueTags(
          parsed.filter((tag): tag is string => typeof tag === "string"),
        )
      : [];
  } catch {
    return [];
  }
};

export const hasEmptyTagAction = (
  actions: Pick<IRuleParameterEdit, "field" | "value">[],
): boolean =>
  actions.some(
    (action) =>
      action.field === "tags" &&
      deserializeActionTags(action.value).length === 0,
  );

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
      if (!isNumericLiteral(value) || !Number.isFinite(Number(value))) {
        return value;
      }

      return convertNumberToCurrency(
        Number(value),
        true,
        currency,
        SignDisplay.Auto,
        intlLocale,
      );
    case "date":
      return formatDate(value);
    case "category":
      return getFormattedCategoryValue(value, categories);
    case "note":
      return value;
    case "tags":
      return deserializeActionTags(value).join(", ");
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
