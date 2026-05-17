import { ComboboxItem } from "@mantine/core";

export interface IRuleParameterEdit {
  id?: string;
  field: string;
  operator: string;
  value: string;
}

export interface IRuleParameterCreateRequest {
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleRequest {
  conditions: IRuleParameterCreateRequest[];
  actions: IRuleParameterCreateRequest[];
}

export interface IRuleParameterResponse {
  id: string;
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleResponse {
  id: string;
  conditions: IRuleParameterResponse[];
  actions: IRuleParameterResponse[];
}

export interface IRuleParameterUpdateRequest {
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleUpdateRequest {
  id: string;
  conditions: IRuleParameterUpdateRequest[];
  actions: IRuleParameterUpdateRequest[];
}

export interface TransactionField {
  label: string;
  value: string;
  appliesTo: TransactionFieldScope;
}

export enum TransactionFieldScope {
  CONDITION = "condition",
  ACTION = "action",
  BOTH = "both",
}

export const TransactionFields: TransactionField[] = [
  {
    label: "merchant_name",
    value: "merchant",
    appliesTo: TransactionFieldScope.BOTH,
  },
  { label: "date", value: "date", appliesTo: TransactionFieldScope.BOTH },
  {
    label: "amount",
    value: "amount",
    appliesTo: TransactionFieldScope.BOTH,
  },
  {
    label: "category",
    value: "category",
    appliesTo: TransactionFieldScope.BOTH,
  },
  {
    label: "account",
    value: "account",
    appliesTo: TransactionFieldScope.CONDITION,
  },
];

export const ConditionTransactionFields: ComboboxItem[] =
  TransactionFields.filter(
    (field) =>
      field.appliesTo === TransactionFieldScope.CONDITION ||
      field.appliesTo === TransactionFieldScope.BOTH,
  ).map(({ label, value }) => ({ label, value }));

export const ActionTransactionFields: ComboboxItem[] = TransactionFields.filter(
  (field) =>
    field.appliesTo === TransactionFieldScope.ACTION ||
    field.appliesTo === TransactionFieldScope.BOTH,
).map(({ label, value }) => ({ label, value }));

export enum OperatorTypes {
  STRING = "string",
  NUMBER = "number",
  DATE = "date",
  CATEGORY = "category",
  STRING_ARRAY = "string_array",
}

export const FieldToOperatorType = new Map<string, OperatorTypes>([
  ["merchant", OperatorTypes.STRING],
  ["date", OperatorTypes.DATE],
  ["amount", OperatorTypes.NUMBER],
  ["category", OperatorTypes.CATEGORY],
  ["account", OperatorTypes.STRING_ARRAY],
]);

export const ActionOperators: Operator[] = [
  { label: "set", value: "set", type: [] },
  { label: "delete_transaction", value: "delete", type: [] },
];

export interface Operator {
  label: string;
  value: string;
  type: OperatorTypes[];
}

export const Operators: Operator[] = [
  {
    label: "equals",
    value: "equals",
    type: [OperatorTypes.STRING, OperatorTypes.NUMBER],
  },
  {
    label: "does_not_equal",
    value: "notEquals",
    type: [OperatorTypes.STRING, OperatorTypes.NUMBER],
  },
  { label: "contains", value: "contains", type: [OperatorTypes.STRING] },
  {
    label: "does_not_contain",
    value: "doesNotContain",
    type: [OperatorTypes.STRING],
  },
  { label: "starts_with", value: "startsWith", type: [OperatorTypes.STRING] },
  { label: "ends_with", value: "endsWith", type: [OperatorTypes.STRING] },
  {
    label: "matches_regex",
    value: "matchesRegex",
    type: [OperatorTypes.STRING],
  },
  { label: "greater_than", value: "greaterThan", type: [OperatorTypes.NUMBER] },
  { label: "less_than", value: "lessThan", type: [OperatorTypes.NUMBER] },
  { label: "on", value: "on", type: [OperatorTypes.DATE] },
  { label: "before", value: "before", type: [OperatorTypes.DATE] },
  { label: "after", value: "after", type: [OperatorTypes.DATE] },
  {
    label: "is",
    value: "is",
    type: [OperatorTypes.CATEGORY, OperatorTypes.STRING_ARRAY],
  },
  {
    label: "is_not",
    value: "isNot",
    type: [OperatorTypes.CATEGORY, OperatorTypes.STRING_ARRAY],
  },
];
