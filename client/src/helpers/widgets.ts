import { areStringsEqual } from "~/helpers/utils";
import {
  INetWorthWidgetCategory,
  INetWorthWidgetConfiguration,
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
} from "~/models/widgetSettings";
import { sumAssetsTotalValue } from "./assets";
import { getAccountsOfTypes, sumAccountsTotalBalance } from "./accounts";
import { IAccountResponse } from "~/models/account";
import { IAssetResponse } from "~/models/asset";
import { ComboboxItem } from "@mantine/core";

/**
 * Return a string representation for the supplied value, defaulting to an empty string.
 *
 * @param value - Value that should be treated as a string.
 * @returns The normalized string (or empty string when parsing fails).
 */
const normalizeString = (value: unknown): string => {
  if (typeof value === "string") {
    return value;
  }

  if (typeof value === "number" && Number.isFinite(value)) {
    return value.toString();
  }

  return "";
};

/**
 * Parse a number from various inputs and fall back to 0 on failure.
 *
 * @param value - The raw value that should become a number.
 * @returns The parsed number or 0 when parsing is impossible.
 */
const normalizeNumber = (value: unknown): number => {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim() !== "") {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  return 0;
};

/**
 * Safely parse JSON text into an object without throwing.
 *
 * @param configuration - Raw JSON string returned by the backend.
 * @returns The parsed object or undefined on failure.
 */
const safeParseJson = (
  configuration: string
): Record<string, unknown> | undefined => {
  try {
    return JSON.parse(configuration) as Record<string, unknown>;
  } catch {
    return undefined;
  }
};

/**
 * Normalize a mixed array of category representations into the typed form.
 *
 * @param categories - Incoming data for categories.
 * @returns An array of normalized widget categories.
 */
const normalizeCategories = (
  categories: unknown
): INetWorthWidgetCategory[] => {
  if (!Array.isArray(categories)) {
    return [];
  }

  return categories.map((category) => {
    const record = (category ?? {}) as Record<string, unknown>;
    return {
      id: normalizeString(record.id ?? record.ID),
      value: normalizeString(record.value ?? record.Value),
      type: normalizeString(record.type ?? record.Type),
      subtype: normalizeString(record.subtype ?? record.Subtype),
    };
  });
};

const normalizeGroups = (groups: unknown): INetWorthWidgetGroup[] => {
  if (!Array.isArray(groups)) {
    return [];
  }

  return groups.map((group) => {
    const record = (group ?? {}) as Record<string, unknown>;
    return {
      id: normalizeString(record.id ?? record.ID),
      index: normalizeNumber(record.index ?? record.Index),
      lines: normalizeLines(record.lines ?? record.Lines),
    };
  });
};

/**
 * Normalize each widget line (including PascalCase vs camelCase keys) to the expected interface.
 *
 * @param lines - Raw line data from the widget configuration.
 * @returns The list of normalized net worth lines.
 */
const normalizeLines = (lines: unknown): INetWorthWidgetLine[] => {
  if (!Array.isArray(lines)) {
    return [];
  }

  return lines.map((line) => {
    const record = (line ?? {}) as Record<string, unknown>;
    return {
      id: normalizeString(record.id ?? record.ID),
      name: normalizeString(record.name ?? record.Name),
      index: normalizeNumber(record.index ?? record.Index),
      categories: normalizeCategories(record.categories ?? record.Categories),
    };
  });
};

/**
 * Parse the Net Worth widget configuration blob from the backend (stringified JSON) and
 * ensure each line conforms to the typed structure.
 *
 * @param configuration - Serialized configuration string from the backend.
 * @returns The normalized configuration or undefined when parsing fails.
 */
export const parseNetWorthConfiguration = (
  configuration?: string
): INetWorthWidgetConfiguration | undefined => {
  if (!configuration) {
    return undefined;
  }

  const parsed = safeParseJson(configuration);
  if (!parsed) {
    return undefined;
  }

  const groupsRaw = parsed.groups ?? parsed.Groups;
  const normalizedGroups = normalizeGroups(groupsRaw);

  if (normalizedGroups.length === 0) {
    return undefined;
  }

  return {
    groups: normalizedGroups,
  };
};

/**
 * Check whether a widgetType string refers to the Net Worth widget.
 *
 * @param widgetType - Raw widget type string from the backend.
 * @returns True when the type matches the Net Worth widget.
 */
export const isNetWorthWidgetType = (widgetType: string): boolean =>
  areStringsEqual(widgetType, "Net Worth") ||
  areStringsEqual(widgetType, "NetWorth");

/**
 * Check whether a given category represents an asset category.
 * @param category - The widget category to check.
 * @returns True if the category is related to assets, false otherwise.
 */
export const isAssetCategory = (category: INetWorthWidgetCategory): boolean =>
  (category.type?.toLowerCase() ?? "").includes("asset");

/**
 * Check whether a given category represents an account category.
 * @param category - The widget category to check.
 * @returns True if the category is related to accounts, false otherwise.
 */
export const isAccountCategory = (category: INetWorthWidgetCategory): boolean =>
  (category.type?.toLowerCase() ?? "").includes("account");

/** Check whether a given category represents a another net worth line.
 * @param category The widget category to check.
 * @returns True if the category is related to lines, false otherwise.
 */
export const isLineCategory = (category: INetWorthWidgetCategory): boolean =>
  (category.type?.toLowerCase() ?? "").includes("line");

/**
 * Get the total value for a given asset category.
 * @param category The net worth widget category.
 * @param assets The list of asset responses.
 * @returns The total value for the category.
 */
export const getAssetValueForCategory = (
  category: INetWorthWidgetCategory,
  assets: IAssetResponse[]
): number => {
  if (areStringsEqual(category.subtype, "all")) {
    return sumAssetsTotalValue(assets);
  }

  // Currently, we only support "all" as a subtype for assets.
  return 0;
};

/**
 * Calculate the total value for a given net worth widget line based on its categories.
 * @param line The net worth widget line to calculate the total for.
 * @param validAccounts The list of valid account responses.
 * @param validAssets The list of valid asset responses.
 * @returns The total value for the line.
 */
export const calculateLineTotal = (
  line: INetWorthWidgetLine,
  validAccounts: IAccountResponse[],
  validAssets: IAssetResponse[],
  lines: INetWorthWidgetLine[]
): number => {
  const categories = line.categories ?? [];

  if (categories.length === 0) {
    return 0;
  }

  return categories.reduce(
    (total: number, category: INetWorthWidgetCategory) => {
      if (isAssetCategory(category)) {
        return total + getAssetValueForCategory(category, validAssets);
      }

      if (isAccountCategory(category)) {
        if (areStringsEqual(category.subtype, "category")) {
          const filters = [category.value]
            .filter(Boolean)
            .map((value) => value as string);

          if (filters.length === 0) {
            return total;
          }

          return (
            total +
            sumAccountsTotalBalance(getAccountsOfTypes(validAccounts, filters))
          );
        }

        return total;
      }

      if (isLineCategory(category)) {
        const lineMatch = lines.find((l) => l.name === category.value);
        if (lineMatch) {
          const lineTotal = calculateLineTotal(
            lineMatch,
            validAccounts,
            validAssets,
            lines
          );
          return total + lineTotal;
        }
      }

      return total;
    },
    0
  );
};

export const NET_WORTH_CATEGORY_TYPES: ComboboxItem[] = [
  { value: "account", label: "account" },
  { value: "asset", label: "asset" },
  { value: "line", label: "line" },
];

export const NET_WORTH_CATEGORY_ACCOUNT_SUBTYPES: ComboboxItem[] = [
  { value: "category", label: "category" },
];

export const NET_WORTH_CATEGORY_ASSET_SUBTYPES: ComboboxItem[] = [
  { value: "all", label: "all" },
];

export const NET_WORTH_CATEGORY_LINE_SUBTYPES: ComboboxItem[] = [
  { value: "name", label: "name" },
];

/**
 * Get the subtype options for a given category type.
 *
 * @param type - The category type.
 * @returns An array of subtype options.
 */
export const getSubtypeOptions = (type: string) => {
  switch (type?.toLowerCase()) {
    case "account":
      return NET_WORTH_CATEGORY_ACCOUNT_SUBTYPES;
    case "asset":
      return NET_WORTH_CATEGORY_ASSET_SUBTYPES;
    case "line":
      return NET_WORTH_CATEGORY_LINE_SUBTYPES;
    default:
      return [];
  }
};
